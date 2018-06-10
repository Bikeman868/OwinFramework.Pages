using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Collections
{
    /// <summary>
    /// Implementation of IArrayFactory that pools and reuses arrays
    /// </summary>
    internal class ArrayFactory: Disposable, IArrayFactory
    {
        private readonly IQueueFactory _queueFactory;
        private readonly long[] _arraySizes;
        private readonly IThreadSafeDictionary<Type, object> _pools;

        public ArrayFactory(IQueueFactory queueFactory)
        {
            _queueFactory = queueFactory;

            // Define standard array sizes, this increases chances of reuse
            _arraySizes = new long[40];
            _arraySizes[0] = 32;
            for (var i = 1; i < _arraySizes.Length; i++)
                _arraySizes[i] = _arraySizes[i - 1] * 2;
            _pools = new ThreadSafeDictionary<Type, object>();
        }

        protected override void Dispose(bool destructor)
        {
            if (!destructor)
            {
                foreach (var listEntry in _pools) 
                    listEntry.Value.Dispose();
                _pools.Dispose();
            }
            base.Dispose(destructor);
        }

        public IArray<T> CreateExact<T>(long size)
        {
            return GetArray(GetPool<T>(size, false));
        }

        public IArray<T> CreateAtLeast<T>(long size)
        {
            return GetArray(GetPool<T>(size, true));
        }

        private IArray<T> GetArray<T>(Pool<T> pool)
        {
            var array = (ReusableArray<T>)pool.DequeueOrDefault();
            if (array == null) array = new ReusableArray<T>(pool.ArraySize);

            return array.Initialize(
                (IReusable reusable) =>
                {
                    var a = (IArray<T>)reusable;
                    if (a.IsReusable) pool.Enqueue(a);
                });
        }

        private Pool<T> GetPool<T>(long size, bool allowLarger)
        {
            var poolList = (SortedList<long, Pool<T>>)_pools.GetOrAdd(typeof(T), t => new SortedList<long, Pool<T>>(), null);

            Pool<T> pool;
            if (poolList.TryGetValue(size, out pool)) return pool;

            lock (poolList)
            {
                if (poolList.TryGetValue(size, out pool)) return pool;
                if (allowLarger)
                {
                    // Look for the first pool which is larger than the requested size
                    pool = poolList.FirstOrDefault(p => p.Key >= size).Value;

                    // If this pool is less than twice the requested size then use it
                    if (pool != null && pool.ArraySize < size * 2) return pool;

                    // Adjust size to the next largest standard array size
                    size = _arraySizes.FirstOrDefault(s => s >= size);

                    // Return if already in the list
                    if (poolList.TryGetValue(size, out pool)) return pool;
                }
                pool = new Pool<T>(size, _queueFactory.Create<IArray<T>>(50));
                poolList.Add(size, pool);
                return pool;
            }
        }

        private class Pool<T> : IDisposable
        {
            public long ArraySize { get; private set; }

            private readonly IQueue<IArray<T>> _arrays;
            private long _hitCount;

            public Pool(long arraySize, IQueue<IArray<T>> arrays)
            {
                ArraySize = arraySize;
                _arrays = arrays;
            }

            public void Dispose()
            {
                _arrays.Dispose();
            }

            public IArray<T> DequeueOrDefault()
            {
                Interlocked.Increment(ref _hitCount);
                return _arrays.DequeueOrDefault();
            }

            public void Enqueue(IArray<T> array)
            {
#if DEBUG
                if (array.Size != ArraySize)
                    throw new ApplicationException("Internal error, attempt to pool reusable array with wrong size");
#endif

                if (ArraySize <= 2000)
                    _arrays.Enqueue(array);
                else
                {
                    var hitCount = Interlocked.CompareExchange(ref _hitCount, -1, -1);
                    if (hitCount > 10) _arrays.Enqueue(array);
                }
            }
        }

    }
}
