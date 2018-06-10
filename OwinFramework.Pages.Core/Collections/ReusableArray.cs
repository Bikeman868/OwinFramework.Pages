using System;
using System.Collections;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Collections
{
    /// <summary>
    /// An implementation of IArray{T} that is re-usable
    /// </summary>
    internal class ReusableArray<T> : ReusableObject, IArray<T>
    {
        private readonly T[] _arrayData;

        public ReusableArray(long size)
        {
            _arrayData = new T[size];
        }

        public long Size { get { return _arrayData.LongLength; } }

        public new IArray<T> Initialize(Action<IReusable> disposeAction)
        {
            base.Initialize(disposeAction);
            return this;
        }

        public T[] GetArray()
        {
            return _arrayData;
        }

        public T this[long index]
        {
            get { return _arrayData[index]; }
            set { _arrayData[index] = value; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return (IEnumerator<T>)_arrayData.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _arrayData.GetEnumerator();
        }
    }
}
