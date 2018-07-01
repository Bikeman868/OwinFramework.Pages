using Microsoft.Owin;

namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// The IoC dependencies are wrapped in this factory so that when
    /// new dependencies are added it does not change the constructor
    /// which would break any application code that inherits from it
    /// </summary>
    public interface IModuleDependenciesFactory
    {
        /// <summary>
        /// Constructs and initializes a module dependencies instance
        /// specific to the request
        /// </summary>
        IModuleDependencies Create(IOwinContext context);
    }
}
