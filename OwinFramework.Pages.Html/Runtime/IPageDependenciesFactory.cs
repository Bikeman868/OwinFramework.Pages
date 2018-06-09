using Microsoft.Owin;

namespace OwinFramework.Pages.Html.Runtime
{
    public interface IPageDependenciesFactory
    {
        /// <summary>
        /// Constructs and initializes a page dependencies instance
        /// </summary>
        IPageDependencies Create(IOwinContext context);
    }
}
