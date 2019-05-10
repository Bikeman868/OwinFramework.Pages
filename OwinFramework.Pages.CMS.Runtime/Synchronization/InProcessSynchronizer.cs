using System;
using System.Linq;
using OwinFramework.Pages.CMS.Runtime.Interfaces;
using OwinFramework.Pages.CMS.Runtime.Interfaces.LiveUpdate;

namespace OwinFramework.Pages.CMS.Runtime.Synchronization
{
    public class InProcessSynchronizer: ILiveUpdateSender, ILiveUpdateReceiver
    {
        private readonly object _lock = new object();
        private ILiveUpdateRecipient[] _subscribers;

        public void Dispose()
        {
        }

        void ILiveUpdateSender.Send(MessageDto updateMessage)
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

        ILiveUpdateRecipient ILiveUpdateReceiver.Subscribe(Action<MessageDto> onMessageReceived)
        {
            var subscriber = new Subscriber(
                unsubscribe =>
                {
                    lock (_lock)
                    {
                        if (_subscribers != null)
                            _subscribers = _subscribers.Where(s => !ReferenceEquals(s, unsubscribe)).ToArray();
                    }
                }, 
                onMessageReceived);

            lock (_lock)
            {
                if (_subscribers == null)
                    _subscribers = new ILiveUpdateRecipient[] { subscriber };
                else
                    _subscribers = _subscribers.Concat(Enumerable.Repeat(subscriber, 1)).ToArray();
            }

            return subscriber;
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
