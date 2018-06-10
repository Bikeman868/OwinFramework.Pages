using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Interfaces.Collections;

namespace OwinFramework.Pages.Core.Collections
{
    /// <summary>
    /// This is a concrete implementation of IEnumerableLocked{T}. If locks the
    /// underlying collection while it is being enumerated.
    /// Implements IDisposable which unlocks the underlying collection.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EnumerableLocked<T> : Disposable, IEnumerableLocked<T>
    {
        private IEnumerable<T> _enumerable;
        private Action _lockAction;
        private Action _unlockAction;
        private bool _locked;

        /// <summary>
        /// You must initialize each instance immediately after construction
        /// </summary>
        /// <param name="enumerable">The collection to enumerate</param>
        /// <param name="lockAction">A lambda expression that locks the underlying collection</param>
        /// <param name="unlockAction">A lambda expression that unlocks the underlying collection</param>
        public IEnumerableLocked<T> Initialize(IEnumerable<T> enumerable, Action lockAction, Action unlockAction)
        {
            _enumerable = enumerable;
            _lockAction = lockAction;
            _unlockAction = unlockAction;

            return this;
        }

        /// <summary>
        /// Locks the underlying collection and gets an enumerator for it
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            if (!_locked && _lockAction != null)
            {
                _lockAction();
                _lockAction = null;
                _locked = true;
            }
            return _enumerable.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Unlocks the underlying collection when this instance is disposed
        /// </summary>
        protected override void Dispose(bool destructor)
        {
            if (_locked && _unlockAction != null)
                _unlockAction();
            _unlockAction = null;

            base.Dispose(destructor);
        }
    }
}
