using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Runtime
{
    public interface IRenderContextFactory
    {
        IRenderContext Create();
    }
}
