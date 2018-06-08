using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Facilities.Collections
{
    /// <summary>
    /// Base class for implementations of re-usable collections that are enumerable
    /// </summary>
    public class ReusableCollection<T> : ReusableObject, IEnumerable<T>
    {
        private class Enumerator : IEnumerator<T>
        {
            readonly ReusableCollection<T> _collection;
            int _index;

            public Enumerator(ReusableCollection<T> collection)
            {
                _collection = collection;
            }

            public T Current
            {
                get { return _collection[_index]; }
            }

            public void Dispose()
            {
            }

            object System.Collections.IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                _index++;
                if (_index >= _collection.Length) return false;
                return true;
            }

            public void Reset()
            {
                _index = -1;
            }
        }

        private const Int64 _initialSize = 8000;
        private readonly IArrayFactory _arrayFactory;

        private IArray<T> _array;
        protected IArray<T> ReusableArray
        {
            get { return _array; }
            set
            {
                if (_array != null) _array.Dispose();
                _array = value;
            }
        }

        private Int64 _length;
        public Int64 Length 
        {
            get { return _length; }
            set
            {
                var newCapacity = GetCapacity();
                while (value > newCapacity) newCapacity = newCapacity < _initialSize ? _initialSize : newCapacity * 2;
                if (newCapacity > GetCapacity()) SetCapacity(newCapacity);
                _length = value;
            }
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= Length) throw new IndexOutOfRangeException();
                return _array[index];
            }
            set
            {
                if (index < 0 || index >= Length) throw new IndexOutOfRangeException();
                _array[index] = value;
            }
        }

        protected ReusableCollection(IArrayFactory arrayFactory)
        {
            _arrayFactory = arrayFactory;
        }

        protected ReusableCollection<T> Initialize(Action<IReusable> disposeAction, Int64 capacity)
        {
            base.Initialize(disposeAction);
            SetCapacity(capacity);
            return this;
        }

        protected override void Dispose(bool destructor)
        {
            if (!destructor)
                Clear();
            base.Dispose(destructor);
        }

        protected IArray<T> ExtractBuffer()
        {
            var result = _array;
            _array = null;
            return result;
        }

        public void Clear()
        {
            _length = 0;
            ReusableArray = null;
        }

        public void Sort(IComparer<T> comparer)
        {
            Array.Sort(ReusableArray.GetArray(), 0, (int)Length, comparer);
        }

        protected Int64 GetCapacity()
        {
            return ReusableArray == null ? 0 : ReusableArray.Size;
        }

        protected void SetCapacity(Int64 capacity)
        {
            if (capacity == 0)
            {
                ReusableArray = null;
            }
            else
            {
                var newArray = _arrayFactory.CreateAtLeast<T>(capacity);
                if (ReusableArray != null)
                {
                    if (newArray.Size > _array.Size)
                        ReusableArray.GetArray().CopyTo(newArray.GetArray(), 0);
                    else
                    {
                        var newArrayData = newArray.GetArray();
                        var oldArray = _array.GetArray();
                        for (var index = 0; index < newArray.Size; index++)
                        {
                            newArrayData[index] = oldArray[index];
                        }
                    }
                }
                ReusableArray = newArray;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
