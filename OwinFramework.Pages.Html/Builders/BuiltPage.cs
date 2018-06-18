using System.Collections.Generic;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Builders
{
    internal class BuiltPage : Page
    {
        private Dictionary<string, IElement> _regions;

        public BuiltPage(IPageDependenciesFactory dependenciesFactory)
            : base(dependenciesFactory)
        {
        }

        public override void Initialize()
        {
            if (_regions != null && _regions.Count > 0)
            {
                if (!Layout.IsClone)
                    Layout = Layout.Clone();

                foreach (var region in _regions)
                    Layout.Populate(region.Key, region.Value);
            }

            base.Initialize();
        }

        public void PopulateRegion(string regionName, IElement element)
        {
            if (_regions == null)
                _regions = new Dictionary<string, IElement>();
            _regions[regionName] = element;
        }
    }
}
