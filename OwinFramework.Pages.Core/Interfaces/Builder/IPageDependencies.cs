using System;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// Defines the injected dependencies of page.They are packaged like this
    /// to avoid changing the constructor signature when new dependencies are added.
    /// </summary>
    public interface IPageDependencies : IDisposable
    {
        /// <summary>
        /// Rendering context of the request
        /// </summary>
        IRenderContext RenderContext { get; }

        /// <summary>
        /// Data binding context
        /// </summary>
        IDataContext DataContext { get; }

        /// <summary>
        /// Asset manager
        /// </summary>
        IAssetManager AssetManager { get; }

        /// <summary>
        /// Name manager
        /// </summary>
        INameManager NameManager { get; }

        /// <summary>
        /// Initializes thisinstance for a specific request
        /// </summary>
        IPageDependencies Initialize(IOwinContext context);
    }
}
