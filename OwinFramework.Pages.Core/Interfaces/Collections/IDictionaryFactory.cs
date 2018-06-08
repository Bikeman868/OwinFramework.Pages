using System.Collections.Generic;

namespace OwinFramework.Pages.Core.Interfaces.Collections
{
    /// <summary>
    /// Defines a factory that creates thread safe dictionaries
    /// </summary>
    public interface IDictionaryFactory
    {
        /// <summary>
        /// Constructs an empty dictionary
        /// </summary>
        /// <typeparam name="TKey">The type of data used to find entries in the dictionary</typeparam>
        /// <typeparam name="TValue">The type of data in the dictionary</typeparam>
        /// <param name="equalityComparer">Specifies how to compare keys</param>
        /// <returns></returns>
        IThreadSafeDictionary<TKey, TValue> Create<TKey, TValue>(IEqualityComparer<TKey> equalityComparer = null);
    }
}
