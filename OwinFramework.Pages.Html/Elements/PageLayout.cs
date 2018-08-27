using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Elements
{
    internal class PageLayout: PageElement
    {
        private readonly IDictionary<string, PageRegion> _regions;

        public PageLayout(
            PageElementDependencies dependencies,
            PageElement parent,
            ILayout layout, 
            IEnumerable<Tuple<IRegion, IElement>> regionElements,
            IPageData pageData)
            : base(dependencies, parent, layout, pageData)
        {
            _regions = dependencies.DictionaryFactory.Create<string, PageRegion>();

            // TODO: regionElements can be null or incomplete

            foreach (var regionElement in regionElements)
            {
                var region = regionElement.Item1;
                var element = regionElement.Item2;

                var pageRegion = new PageRegion(dependencies, this, region, element, pageData);
                _regions[region.Name] = pageRegion;
            }

            Children = _regions.Values.ToArray();
        }

        protected override IWriteResult WritePageAreaInternal(
            IRenderContext renderContext, 
            IDataContextBuilder dataContextBuilder, 
            PageArea pageArea)
        {
            var layout = Element as ILayout;
            if (ReferenceEquals(layout, null)) return WriteResult.Continue();

            return layout.WritePageArea(renderContext, dataContextBuilder, pageArea, WriteRegion);
        }

        private IWriteResult WriteRegion(
            IRenderContext renderContext,
            IDataContextBuilder dataContextBuilder,
            PageArea pageArea,
            string regionName)
        {
            var pageRegion = _regions[regionName];
            return pageRegion.WritePageArea(renderContext, dataContextBuilder, pageArea);
        }
    }
}
