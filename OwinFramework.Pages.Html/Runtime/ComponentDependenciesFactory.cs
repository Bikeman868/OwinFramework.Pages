using OwinFramework.Pages.Core.Interfaces.Collections;

namespace OwinFramework.Pages.Html.Runtime
{
    internal class ComponentDependenciesFactory: IComponentDependenciesFactory
    {
        public IComponentDependencies Create()
        {
            return new ComponentDependencies();
        }
    }
}
