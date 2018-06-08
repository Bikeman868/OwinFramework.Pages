using System;
using System.Collections.Generic;

namespace OwinFramework.Pages.Core.Interfaces.Collections
{
    /// <summary>
    /// A dictionary that is fully thread safe and also provides atomic operations
    /// such as add if not already present
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary key</typeparam>
    /// <typeparam name="TValue">The type of data stored in the dictionary</typeparam>
    public interface IThreadSafeDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        /// <summary>
        /// Gets a value from the dictionary if it contains it, otherwise adds the
        /// value to the dictionary and returns it in a thread-safe manner.
        /// </summary>
        /// <param name="key">The key to look for in the dictionary</param>
        /// <param name="createValueFunction">Lambda expression that will create a new value
        /// to add to the cache. This function is called with the dictionary locked and
        /// must return immediately</param>
        /// <param name="initializeAction">Lambda expression to initialize a new value
        /// that was just added to the dictionary. This action is invoked with the dictionary
        /// unlocked, so it can call out to databases or services without blocking other threads</param>
        TValue GetOrAdd(TKey key, Func<TKey, TValue> createValueFunction, Action<TKey, TValue> initializeAction);

        /// <summary>
        /// IMPORTANT: you must Dispose() of the enumerable to unlock the dictionary
        /// </summary>
        IEnumerableLocked<TKey> KeysLocked { get; }

        /// <summary>
        /// IMPORTANT: you must Dispose() of the enumerable to unlock the dictionary
        /// </summary>
        IEnumerableLocked<TValue> ValuesLocked { get; }

        /// <summary>
        /// IMPORTANT: you must Dispose() of the enumerable to unlock the dictionary
        /// </summary>
        IEnumerableLocked<KeyValuePair<TKey, TValue>> KeyValuePairsLocked { get; }

        /// <summary>
        /// Returns the key at the specified index, and increments the index.
        /// Rolls the index over to 0 if it is beyond the end of the dictionary.
        /// To enumerate all keys in the dictionary initialize the index to 1 and
        /// call this method until the value is 1 again.
        /// </summary>
        TKey GetNextKey(ref int index);

        /// <summary>
        /// Returns the value at the specified index, and increments the index.
        /// Rolls the index over to 0 if it is beyond the end of the dictionary.
        /// To enumerate all keys in the dictionary initialize the index to 1 and
        /// call this method until the value is 1 again.
        /// </summary>
        TValue GetNextValue(ref int index);

        /// <summary>
        /// Performs a thread-safe atomic read/modify/write of an entry in the
        /// dictionary. The item does not have to already exist in the dictionary
        /// </summary>
        /// <param name="key">The key that identifies the entry to update</param>
        /// <param name="updateMethod">A function to calculate a new value. Note that
        /// this function is called with the dictionary locked so this function
        /// must return immediately. Note that if the key is not found in the
        /// dictionary this function will be passed default(T) and the returned
        /// value will be added to the dictionary</param>
        /// <returns>The original value that was replaced</returns>
        TValue Update(TKey key, Func<TValue, TValue> updateMethod);
    }
}
