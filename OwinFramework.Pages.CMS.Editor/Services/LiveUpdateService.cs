using System;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;
using OwinFramework.Pages.CMS.Runtime.Interfaces;
using OwinFramework.Pages.CMS.Runtime.Interfaces.LiveUpdate;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Restful.Interfaces;
using OwinFramework.Pages.Restful.Parameters;

namespace OwinFramework.Pages.CMS.Editor.Services
{
    internal class LiveUpdateService
    {
        private readonly ILiveUpdateReceiver _liveUpdateReceiver;

        private Dictionary<long, Client> _connectedClients;
        private long _nextId;

        public LiveUpdateService(ILiveUpdateReceiver liveUpdateReceiver)
        {
            _liveUpdateReceiver = liveUpdateReceiver;
            _connectedClients = new Dictionary<long, Client>();
            _nextId = 1;
        }

        [Endpoint]
        public void Register(IEndpointRequest request)
        {
            var response = new Response
            {
                Id = Interlocked.Increment(ref _nextId),
                Success = true
            };

            lock (_connectedClients)
            {
                var client = new Client(response.Id, _liveUpdateReceiver);
                _connectedClients.Add(response.Id, client);
            }

            request.Success(response);
        }

        [Endpoint]
        [EndpointParameter("id", typeof(AnyValue<long>))]
        public void Deregister(IEndpointRequest request)
        {
            var id = request.Parameter<long>("id");

            var response = new Response
            {
                Id = id,
                Success = true
            };

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

        [Endpoint]
        [EndpointParameter("id", typeof(AnyValue<long>))]
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
                _maximumWaitTime = TimeSpan.FromMinutes(1);
                _expiryTime = TimeSpan.FromMinutes(10);
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
                        _response = new Response();
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
