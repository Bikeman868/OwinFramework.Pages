using System;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Runtime
{
    public interface IPageDependencies: IDisposable
    {
        IRenderContext RenderContext { get; }
        IDataContext DataContext { get; }
        IAssetManager AssetManager { get; }
        INameManager NameManager { get; }

        IPageDependencies Initialize(IOwinContext context);
    }
}
