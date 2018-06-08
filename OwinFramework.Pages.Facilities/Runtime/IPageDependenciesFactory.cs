using Microsoft.Owin;

namespace OwinFramework.Pages.Facilities.Runtime
{
    public interface IPageDependenciesFactory
    {
        /// <summary>
        /// Constructs but does not initialize the page dependencies
        /// </summary>
        IPageDependencies Create();
    }
}
