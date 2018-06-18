using Microsoft.Owin;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Managers;

namespace OwinFramework.Pages.Html.Runtime
{
    /// <summary>
    /// The IoC dependencies are wrapped in this factory so that when
    /// new dependencies are added it does not change the constructor
    /// which would break any application code that inherits from it
    /// </summary>
    public interface ILayoutDependenciesFactory
    {
        /// <summary>
        /// Constructs and initializes a layout dependencies instance
        /// </summary>
        ILayoutDependencies Create();

        /// <summary>
        /// The dictionary factory is a singleton and therefore alwaya available
        /// </summary>
        IDictionaryFactory DictionaryFactory { get; }
    }
}
