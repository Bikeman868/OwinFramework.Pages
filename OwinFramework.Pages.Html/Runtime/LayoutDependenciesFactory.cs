using Microsoft.Owin;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.DataModel;

namespace OwinFramework.Pages.Html.Runtime
{
    internal class LayoutDependenciesFactory: ILayoutDependenciesFactory
    {
        public IDataConsumerFactory DataConsumerFactory { get; private set; }
        public IDictionaryFactory DictionaryFactory { get; private set; }

        public LayoutDependenciesFactory(
            IDictionaryFactory dictionaryFactory,
            IDataConsumerFactory dataConsumerFactory)
        {
            DictionaryFactory = dictionaryFactory;
            DataConsumerFactory = dataConsumerFactory;
        }

        public ILayoutDependencies Create(IOwinContext context)
        {
            return new LayoutDependencies(DictionaryFactory);
        }
    }
}
