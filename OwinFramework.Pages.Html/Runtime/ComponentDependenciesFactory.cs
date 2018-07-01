using Microsoft.Owin;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;

namespace OwinFramework.Pages.Html.Runtime
{
    internal class ComponentDependenciesFactory: IComponentDependenciesFactory
    {
        public IDataConsumerFactory DataConsumerFactory { get; private set; }

        public ComponentDependenciesFactory(
            IDataConsumerFactory dataConsumerFactory)
        {
            DataConsumerFactory = dataConsumerFactory;
        }

        public IComponentDependencies Create(IOwinContext context)
        {
            return new ComponentDependencies();
        }

    }
}
