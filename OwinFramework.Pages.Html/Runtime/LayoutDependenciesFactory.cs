using OwinFramework.Pages.Core.Interfaces.Collections;

namespace OwinFramework.Pages.Html.Runtime
{
    internal class LayoutDependenciesFactory: ILayoutDependenciesFactory
    {
        private readonly IDictionaryFactory _dictionaryFactory;

        public LayoutDependenciesFactory(
            IDictionaryFactory nameManager)
        {
            _dictionaryFactory = nameManager;
        }

        public ILayoutDependencies Create()
        {
            return new LayoutDependencies(_dictionaryFactory);
        }

        public IDictionaryFactory DictionaryFactory
        {
            get { return _dictionaryFactory; }
        }
    }
}
