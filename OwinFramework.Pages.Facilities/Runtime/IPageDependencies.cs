using System;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Facilities.Runtime
{
    public interface IPageDependencies: IDisposable
    {
        IRenderContext RenderContext { get; }
        IDataContext DataContext { get; }

        IPageDependencies Initialize(IOwinContext context);
    }
}
