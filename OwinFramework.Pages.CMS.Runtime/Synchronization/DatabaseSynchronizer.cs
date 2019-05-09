using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using OwinFramework.Pages.CMS.Runtime.Configuration;
using OwinFramework.Pages.CMS.Runtime.Interfaces;
using OwinFramework.Pages.CMS.Runtime.Interfaces.LiveUpdate;
using Prius.Contracts.Interfaces;
using Urchin.Client.Interfaces;
using ParameterDirection = Prius.Contracts.Attributes.ParameterDirection;

namespace OwinFramework.Pages.CMS.Runtime.Synchronization
{
    public class DatabaseSynchronizer: ILiveUpdateSender, ILiveUpdateReceiver, IDisposable
    {
        private readonly IContextFactory _contextFactory;
        private readonly ICommandFactory _commandFactory;

        private readonly object _lock = new object();
        private readonly TimeSpan _idlePollInterval;
        private readonly Thread _pollingThread;

        private bool _disposed;
        private ILiveUpdateRecipient[] _subscribers;
        private long _lastMessageId;

        private HashSet<Guid> _sentMessagesCurrent;
        private HashSet<Guid> _sentMessagesPrior;
        private DateTime _nextHashSetSwap;
        private readonly int _hashSetSwapMinutes;

        private IDisposable _configReg;
        private CmsConfiguration _configuration;

        public DatabaseSynchronizer(
            IConfigurationStore configurationStore,
            IContextFactory contextFactory,
            ICommandFactory commandFactory)
        {
            _contextFactory = contextFactory;
            _commandFactory = commandFactory;

            configurationStore.Register("/owinFramework/pages/cms", c => _configuration = c, new CmsConfiguration());

            _lastMessageId = GetLastMessageId();
            _idlePollInterval = TimeSpan.FromSeconds(5);

            _sentMessagesCurrent = new HashSet<Guid>();
            _sentMessagesPrior = new HashSet<Guid>();
            _hashSetSwapMinutes = 5;
            _nextHashSetSwap = DateTime.UtcNow.AddMinutes(_hashSetSwapMinutes);

            _pollingThread = new Thread(PollThreadEntry)
            {
                Name = "Poll change events",
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal
            };

            _pollingThread.Start();
        }

        public void Dispose()
        {
            _disposed = true;

            if (!_pollingThread.Join(TimeSpan.FromSeconds(15)))
                _pollingThread.Abort();
        }

        void ILiveUpdateSender.Send(MessageDto message)
        {
            _sentMessagesCurrent.Add(message.UniqueId);

            NotifySubscribers(message);
            AppendToDatabase(message);
        }

        ILiveUpdateRecipient ILiveUpdateReceiver.Subscribe(Action<MessageDto> onMessageReceived)
        {
            var subscriber = new Subscriber(
                unsubscribe =>
                {
                    lock (_lock)
                    {
                        _subscribers = _subscribers.Where(s => !ReferenceEquals(s, unsubscribe)).ToArray();
                    }
                }, 
                onMessageReceived);

            lock (_lock)
            {
                _subscribers = _subscribers.Concat(Enumerable.Repeat(subscriber, 1)).ToArray();
            }

            return subscriber;
        }

        private void PollThreadEntry()
        {
            while (!_disposed)
            {
                try
                {
                    Thread.Sleep(1);

                    if (DateTime.UtcNow > _nextHashSetSwap)
                    {
                        _sentMessagesPrior = _sentMessagesCurrent;
                        _sentMessagesCurrent = new HashSet<Guid>();
                        _nextHashSetSwap = DateTime.UtcNow.AddMinutes(_hashSetSwapMinutes);
                    }

                    var message = GetNextMessage(ref _lastMessageId);
                    if (message == null)
                    {
                        Thread.Sleep(_idlePollInterval);
                    }
                    else
                    {
                        if (_sentMessagesPrior.Contains(message.UniqueId)) continue;
                        if (_sentMessagesCurrent.Contains(message.UniqueId)) continue;

                        NotifySubscribers(message);
                    }
                }
                catch (ThreadAbortException)
                {
                    return;
                }
                catch
                {
                    Thread.Sleep(TimeSpan.FromSeconds(10));
                }
            }
        }

        private void AppendToDatabase(MessageDto message)
        {
            var json = JsonConvert.SerializeObject(message);

            using (var command = _commandFactory.CreateStoredProcedure("sp_InsertMessage"))
            {
                command.AddParameter("message", json);
                using (var context = _contextFactory.Create(_configuration.LiveUpdateRepositoryName))
                {
                    context.ExecuteNonQuery(command);
                }
            }
        }

        private long GetLastMessageId()
        {
            using (var command = _commandFactory.CreateStoredProcedure("sp_GetLastMessageId"))
            {
                var messageIdParameter = command.AddParameter("messageId", SqlDbType.BigInt);
                using (var context = _contextFactory.Create(_configuration.LiveUpdateRepositoryName))
                {
                    context.ExecuteNonQuery(command);
                    return (long)messageIdParameter.Value;
                }
            }
        }

        private MessageDto GetNextMessage(ref long lastMessageId)
        {
            using (var command = _commandFactory.CreateStoredProcedure("sp_GetNextMessage"))
            {
                command.AddParameter("messageId", lastMessageId);
                using (var context = _contextFactory.Create(_configuration.LiveUpdateRepositoryName))
                {
                    using (var reader = context.ExecuteReader(command))
                    {
                        if (reader.Read())
                        {
                            lastMessageId = reader.Get(0, lastMessageId);
                            var json = reader.Get<string>(1);
                            return JsonConvert.DeserializeObject<MessageDto>(json);
                        }
                    }
                }
            }
            return null;
        }

        void NotifySubscribers(MessageDto updateMessage)
        {
            var subscribers = _subscribers;

            if (subscribers == null || subscribers.Length == 0)
                return;

            foreach (var subscriber in subscribers)
            {
                var handler = subscriber.OnMessageReceived;
                if (handler != null) handler(updateMessage);
            }
        }

        private class Subscriber : ILiveUpdateRecipient
        {
            private Action<MessageDto> _onMessageReceived;
            private Action<Subscriber> _unsubscribeAction;

            Action<MessageDto> ILiveUpdateRecipient.OnMessageReceived
            {
                get { return _onMessageReceived; }
                set { _onMessageReceived = value; }
            }

            public Subscriber(
                Action<Subscriber> unsubscribeAction,
                Action<MessageDto> onMessageReceived)
            {
                _unsubscribeAction = unsubscribeAction;
                _onMessageReceived = onMessageReceived;
            }

            void IDisposable.Dispose()
            {
                if (_unsubscribeAction != null)
                    _unsubscribeAction(this);

                _unsubscribeAction = null;
            }
        }
    }
}
