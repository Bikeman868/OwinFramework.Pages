using Microsoft.Owin;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Runtime
{
    internal class ComponentDependenciesFactory: IComponentDependenciesFactory
    {
        public IDataConsumerFactory DataConsumerFactory { get; private set; }
        public ICssWriterFactory CssWriterFactory { get; private set; }
        public IJavascriptWriterFactory JavascriptWriterFactory { get; private set; }

        public ComponentDependenciesFactory(
            IDataConsumerFactory dataConsumerFactory,
            ICssWriterFactory cssWriterFactory,
            IJavascriptWriterFactory javascriptWriterFactory)
        {
            DataConsumerFactory = dataConsumerFactory;
            CssWriterFactory = cssWriterFactory;
            JavascriptWriterFactory = javascriptWriterFactory;
        }

        public IComponentDependencies Create(IOwinContext context)
        {
            return new ComponentDependencies();
        }

    }
}
