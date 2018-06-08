using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Facilities.Runtime
{
    public interface IRenderContextFactory
    {
        IRenderContext Create();
    }
}
