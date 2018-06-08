using System;
using System.Collections.Generic;
using System.Threading;
using OwinFramework.Pages.Core.Interfaces.Collections;

namespace OwinFramework.Pages.Facilities.Collections
{
    internal class ThreadSafeQueue<T> : Disposable, IQueue<T>
    {
        private Queue<T> _queue;
        private readonly AutoResetEvent _notEmptyEvent;

        private int _capacity;

        public ThreadSafeQueue()
        {
            _notEmptyEvent = new AutoResetEvent(false);
        }

        public IQueue<T> Initialize(int capacity)
        {
            _capacity = capacity;
            _queue = new Queue<T>(capacity);
            return this;
        }

        public void Clear()
        {
            lock (_queue) _queue.Clear();
        }

        public void Clear(Action<T> disposeAction)
        {
            lock(_queue)
            {
                foreach (var item in _queue) 
                    disposeAction(item);
                _queue.Clear();
            }
        }

        public Int32 Count { get { lock (_queue) return _queue.Count; } }

        public Int32 Capacity { get { return _capacity; } set { _capacity = value; } }

        public bool Enqueue(T o)
        {
            lock (_queue)
            {
                if (_capacity > 0 && _queue.Count >= _capacity)
                    return false;

                _queue.Enqueue(o);
                _notEmptyEvent.Set();
            }
            return true;
        }

        public bool Enqueue(IEnumerable<T> items)
        {
            var enumerator = items.GetEnumerator();

            while (enumerator.MoveNext())
            {
                if (!Enqueue(enumerator.Current))
                    return false;
            }
            return true;
        }

        public T Dequeue(TimeSpan timeout)
        {
            if (timeout == TimeSpan.Zero) return DequeueOrDefault();

            while (true)
            {
                if (!_notEmptyEvent.WaitOne(timeout, false)) return default(T);
                lock (_queue)
                {
                    var count = _queue.Count;
                    if (count > 0)
                    {
                        if (count > 1) _notEmptyEvent.Set();
                        return _queue.Dequeue();
                    }
                }
            }
        }

        public T DequeueOrDefault()
        {
            lock (_queue)
            {
                if (_queue.Count == 0) return default(T);
                if (_queue.Count == 1) _notEmptyEvent.Reset();
                return _queue.Dequeue();
            }
        }
    }

}