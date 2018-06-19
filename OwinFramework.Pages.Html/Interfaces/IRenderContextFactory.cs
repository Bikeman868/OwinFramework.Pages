using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Interfaces
{
    public interface IRenderContextFactory
    {
        IRenderContext Create();
    }
}
