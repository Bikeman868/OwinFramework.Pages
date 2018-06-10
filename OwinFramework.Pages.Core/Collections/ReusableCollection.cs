using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Collections
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

        private const Int64 InitialSize = 8000;
        private readonly IArrayFactory _arrayFactory;

        private IArray<T> _array;

        /// <summary>
        /// This is the underlying array that contains the instances that are
        /// part of this collection
        /// </summary>
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

        /// <summary>
        /// Gets the number of instances in this collection
        /// On set, increases the minimum capacity to accomodate tha set length
        /// </summary>
        public Int64 Length 
        {
            get { return _length; }
            set
            {
                var newCapacity = GetCapacity();
                while (value > newCapacity) newCapacity = newCapacity < InitialSize ? InitialSize : newCapacity * 2;
                if (newCapacity > GetCapacity()) SetCapacity(newCapacity);
                _length = value;
            }
        }

        /// <summary>
        /// Provides access to individual instances by their position within the collecxction
        /// </summary>
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

        /// <summary>
        /// You must call this base constructor from your derrived class
        /// </summary>
        protected ReusableCollection(IArrayFactory arrayFactory)
        {
            _arrayFactory = arrayFactory;
        }

        /// <summary>
        /// You must initialialize this base class after construction
        /// </summary>
        protected ReusableCollection<T> Initialize(Action<IReusable> disposeAction, Int64 capacity)
        {
            base.Initialize(disposeAction);
            SetCapacity(capacity);
            return this;
        }

        /// <summary>
        /// Puts this collection back into the pool to be reused
        /// </summary>
        protected override void Dispose(bool destructor)
        {
            if (!destructor)
                Clear();
            base.Dispose(destructor);
        }

        /// <summary>
        /// Removes the underlying collection of instances. After calling this method
        /// you can not use this instance again until you call the Initialize method
        /// </summary>
        /// <returns></returns>
        protected IArray<T> ExtractBuffer()
        {
            var result = _array;
            _array = null;
            return result;
        }

        /// <summary>
        /// Deletes all of the instances in this collection
        /// </summary>
        public void Clear()
        {
            _length = 0;
            ReusableArray = null;
        }

        /// <summary>
        /// Sorts the collection using the provided comparer
        /// </summary>
        public void Sort(IComparer<T> comparer)
        {
            Array.Sort(ReusableArray.GetArray(), 0, (int)Length, comparer);
        }

        /// <summary>
        /// Returns the current capacity of the collection
        /// </summary>
        protected Int64 GetCapacity()
        {
            return ReusableArray == null ? 0 : ReusableArray.Size;
        }

        /// <summary>
        /// Sets the capacity of the collection preserving all of the
        /// existing instances by copying them over to the new structure.
        /// This is an expensive operation that should be avoided whenever
        /// possible.
        /// </summary>
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

        /// <summary>
        /// Gets an enumerator this this collection
        /// </summary>
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
