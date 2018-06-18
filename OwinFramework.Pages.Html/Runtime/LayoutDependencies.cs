using OwinFramework.Pages.Core.Interfaces.Collections;

namespace OwinFramework.Pages.Html.Runtime
{
    internal class LayoutDependencies: ILayoutDependencies
    {
        public IDictionaryFactory DictionaryFactory { get; private set; }

        public LayoutDependencies(
            IDictionaryFactory dictionaryFactory)
        {
            DictionaryFactory = dictionaryFactory;
        }

        public void Dispose()
        {
        }
    }
}
