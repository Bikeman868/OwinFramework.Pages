using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Elements
{
    internal class PageLayout: PageElement
    {
        private readonly IThreadSafeDictionary<string, PageRegion> _regions;

        public PageLayout(
            PageElementDependencies dependencies,
            PageElement parent,
            ILayout layout, 
            IEnumerable<Tuple<string, IRegion, IElement>> regionElements,
            IPageData pageData)
            : base(dependencies, parent, layout, pageData)
        {
            pageData.BeginAddElement(Element);

            _regions = dependencies.DictionaryFactory.Create<string, PageRegion>();

            var regionElementList = regionElements == null
                ? new List<Tuple<string, IRegion, IElement>>() 
                : regionElements.ToList();

            foreach(var regionName in layout.GetRegionNames())
            {
                var name = regionName;
                var regionElement = regionElementList.FirstOrDefault(
                    re => string.Equals(re.Item1, name, StringComparison.OrdinalIgnoreCase));

                var region = regionElement == null ? layout.GetRegion(name) : regionElement.Item2;
                var element = regionElement == null ? layout.GetElement(name) : regionElement.Item3;

                var pageRegion = new PageRegion(dependencies, this, region, element, pageData);
                _regions[regionName] = pageRegion;
            }

            Children = _regions.Values.ToArray();

            pageData.EndAddElement(Element);
        }

        protected override DebugInfo PopulateDebugInfo(DebugInfo debugInfo, int parentDepth, int childDepth)
        {
            var debugLayout = debugInfo as DebugLayout ?? new DebugLayout();

            if (childDepth != 0)
            {
                debugLayout.Children = _regions
                    .Select(kvp =>
                        new DebugLayoutRegion
                        {
                            Name = kvp.Key,
                            Instance = kvp.Value,
                            Region = kvp.Value.GetDebugInfo<DebugRegion>(0, childDepth - 1)
                        })
                    .Cast<DebugInfo>()
                    .ToList();
            }

            return base.PopulateDebugInfo(debugLayout, parentDepth, childDepth);
        }

        protected override IWriteResult WritePageAreaInternal(
            IRenderContext renderContext, 
            PageArea pageArea)
        {
            var layout = Element as ILayout;
            if (ReferenceEquals(layout, null)) return WriteResult.Continue();

            return layout.WritePageArea(renderContext, pageArea, WriteRegion);
        }

        private IWriteResult WriteRegion(
            IRenderContext renderContext,
            PageArea pageArea,
            string regionName)
        {
            var pageRegion = _regions[regionName];
            return pageRegion.WritePageArea(renderContext, pageArea);
        }
    }
}
