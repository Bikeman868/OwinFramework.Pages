using System;
using OwinFramework.Pages.Core.Interfaces.Collections;

namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// Defines the injected dependencies of Layout.They are packaged like this
    /// to avoid changing the constructor signature when new dependencies are added.
    /// </summary>
    public interface ILayoutDependencies : IDisposable
    {
        /// <summary>
        /// A factory for dictionaries
        /// </summary>
        IDictionaryFactory DictionaryFactory { get; }
    }
}
