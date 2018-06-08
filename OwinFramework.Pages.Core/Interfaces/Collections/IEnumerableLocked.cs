using System;
using System.Collections.Generic;

namespace OwinFramework.Pages.Core.Interfaces.Collections
{
    /// <summary>
    /// Defines an enumerable list that can be disposed to unlock the underlying collection
    /// </summary>
    public interface IEnumerableLocked<T> : IEnumerable<T>, IDisposable
    {
    }
}
