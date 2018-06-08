using System.Collections.Generic;
using OwinFramework.Pages.Core.Interfaces.Collections;

namespace OwinFramework.Pages.Facilities.Collections
{
    public class DictionaryFactory : IDictionaryFactory
    {
        public IThreadSafeDictionary<TKey, TValue> Create<TKey, TValue>(IEqualityComparer<TKey> equalityComparer)
        {
            return equalityComparer == null
                ? new ThreadSafeDictionary<TKey, TValue>()
                : new ThreadSafeDictionary<TKey, TValue>(equalityComparer);
        }
    }
}
