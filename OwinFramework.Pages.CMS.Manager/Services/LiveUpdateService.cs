using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using OwinFramework.Pages.CMS.Runtime.Interfaces;
using OwinFramework.Pages.CMS.Runtime.Interfaces.LiveUpdate;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Restful.Interfaces;
using OwinFramework.Pages.Restful.Parameters;

namespace OwinFramework.Pages.CMS.Manager.Services
{
    internal class LiveUpdateService
    {
        private readonly ILiveUpdateReceiver _liveUpdateReceiver;

        private readonly Dictionary<long, Client> _connectedClients;
        private long _nextId;
        private Thread _expiryThread;

        public LiveUpdateService(ILiveUpdateReceiver liveUpdateReceiver)
        {
            _liveUpdateReceiver = liveUpdateReceiver;
            _connectedClients = new Dictionary<long, Client>();

            // Randomize nextid so that if the server is restarted previously
            // connected clients will not likely kill sessions from newly
            // connected clients
            _nextId = new Random().Next();

            _expiryThread = new Thread(() =>
            {
                var count = 60;
                while (true)
                {
                    try
                    {
                        Thread.Sleep(1000);
                        if (--count == 0)
                        {
                            count = 60;
                            lock (_connectedClients)
                            {
                                var ids = _connectedClients.Keys.ToList();
                                foreach (var id in ids)
                                {
                                    var client = _connectedClients[id];
                                    if (client.Expired)
                                    {
#if DEBUG
                                        Trace.WriteLine("Removing expired LiveUpdate session #" + id);
#endif
                                        _connectedClients.Remove(id);
                                        client.Dispose();
                                    }
                                }
                            }
                        }
                    }
                    catch
                    { }
                }
            })
            {
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal,
                Name = "LiveUpdate expiry"
            };

            _expiryThread.Start();
        }

        [Endpoint(UrlPath = "session", Methods = new[]{ Method.Post })]
        public void Register(IEndpointRequest request)
        {
            var response = new Response
            {
                Id = Interlocked.Increment(ref _nextId),
                Success = true
            };

#if DEBUG
            Trace.WriteLine("New LiveUpdate registration #" + response.Id);
#endif

            lock (_connectedClients)
            {
                var client = new Client(response.Id, _liveUpdateReceiver);
                _connectedClients.Add(response.Id, client);
            }

            request.Success(response);
        }

        [Endpoint(UrlPath = "session/{id}", Methods = new[]{ Method.Delete })]
        [EndpointParameter("id", typeof(AnyValue<long>), EndpointParameterType.PathSegment)]
        public void Deregister(IEndpointRequest request)
        {
            var id = request.Parameter<long>("id");

            var response = new Response
            {
                Id = id,
                Success = true
            };

#if DEBUG
            Trace.WriteLine("LiveUpdate de-registration #" + id);
#endif
            lock (_connectedClients)
            {
                Client client;
                if (_connectedClients.TryGetValue(id, out client))
                {
                    _connectedClients.Remove(id);
                    client.Dispose();
                }
            }

            request.Success(response);
        }

        [Endpoint(UrlPath = "updates/{id}")]
        [EndpointParameter("id", typeof(AnyValue<long>), EndpointParameterType.PathSegment)]
        public void Poll(IEndpointRequest request)
        {
            var id = request.Parameter<long>("id");

            Client client;

            lock (_connectedClients)
            {
                if (!_connectedClients.TryGetValue(id, out client))
                {
                    request.Success(new Response
                    {
                        Id = id,
                        Success = false,
                        ErrorMessage = "Session expired, please refresh the page"
                    });
                    return;
                }
            }

            client.Poll(request);
        }

        private class Response
        {
            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("error")]
            public string ErrorMessage { get; set; }

            [JsonProperty("id")]
            public long Id { get; set; }

            [JsonProperty("messages")]
            public List<MessageDto> Messages { get; set; } 
        }

        private class Client: IDisposable
        {
            private readonly long _id;
            private readonly ManualResetEventSlim _responseEvent;
            private readonly TimeSpan _maximumWaitTime;
            private readonly TimeSpan _expiryTime;
            private readonly object _lock;

            private IDisposable _subscription;
            private DateTime _expiry;
            private Response _response;

            public bool Expired
            {
                get { return DateTime.UtcNow > _expiry; }
            }

            public Client(
                long id,
                ILiveUpdateReceiver liveUpdateReceiver)
            {
                _id = id;
                _responseEvent = new ManualResetEventSlim(false);
                _maximumWaitTime = TimeSpan.FromSeconds(60);
                _expiryTime = TimeSpan.FromMinutes(30);
                _lock = new object();

                _subscription = liveUpdateReceiver.Subscribe(OnMessageReceived);
                SetExpiry();
            }

            public void Dispose()
            {
                if (_subscription != null)
                {
                    _subscription.Dispose();
                    _subscription = null;

                    lock (_lock) _response = _response ?? NewResponse();

                    _response.ErrorMessage = "Idle subscription terminated";
                    _response.Success = false;
                    _responseEvent.Set();
                }
            }

            public void Poll(IEndpointRequest request)
            {
                SetExpiry();

                _responseEvent.Wait(_maximumWaitTime);

                Response response;

                lock (_lock)
                {
                    response = _response ?? NewResponse();
                    _response = null;
                    _responseEvent.Reset();
                }

                request.Success(response);
            }

            private void OnMessageReceived(MessageDto message)
            {
                lock (_lock)
                {
                    if (_response == null)
                        _response = NewResponse();
                    _response.Messages.Add(message);
                }

                _responseEvent.Set();
            }

            private void SetExpiry()
            {
                _expiry = DateTime.UtcNow + _expiryTime;
            }

            private Response NewResponse()
            {
                return new Response
                {
                    Id = _id,
                    Success = true,
                    Messages = new List<MessageDto>()
                };
            }
        }
    }
}
