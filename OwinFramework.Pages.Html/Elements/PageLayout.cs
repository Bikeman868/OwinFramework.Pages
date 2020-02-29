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
    /// <summary>
    /// This class is responsible for rendering an ILayout onto a page
    /// </summary>
    internal class PageLayout: PageElement
    {
        private readonly IThreadSafeDictionary<string, PageRegionElement> _pageLayoutZones;

        public PageLayout(
            PageElementDependencies dependencies,
            PageElement parent,
            ILayout layout, 
            IEnumerable<Tuple<string, IRegion, IElement>> regionElements,
            IPageData pageData)
            : base(dependencies, parent, layout, pageData)
        {
            pageData.BeginAddElement(Element);

            _pageLayoutZones = dependencies.DictionaryFactory.Create<string, PageRegionElement>(StringComparer.OrdinalIgnoreCase);

            var regionElementList = regionElements == null
                ? new List<Tuple<string, IRegion, IElement>>() 
                : regionElements.ToList();

            foreach(var zoneName in layout.GetZoneNames())
            {
                var name = zoneName;
                var regionElement = regionElementList.FirstOrDefault(
                    re => string.Equals(re.Item1, name, StringComparison.OrdinalIgnoreCase));

                var region = regionElement == null || regionElement.Item2 == null ? layout.GetRegion(name) : regionElement.Item2;
                var element = regionElement == null || regionElement.Item3 == null ? layout.GetElement(name) : regionElement.Item3;

                _pageLayoutZones[zoneName] = new PageRegionElement(dependencies, this, region, element, pageData);
            }

            Children = _pageLayoutZones.Values.ToArray();

            pageData.EndAddElement(Element);
        }

        protected override T PopulateDebugInfo<T>(DebugInfo debugInfo, int parentDepth, int childDepth)
        {
            var debugLayout = debugInfo as DebugLayout ?? new DebugLayout();

            if (childDepth != 0)
            {
                debugLayout.Children = _pageLayoutZones
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

            return base.PopulateDebugInfo<T>(debugLayout, parentDepth, childDepth);
        }

        protected override IWriteResult WritePageAreaInternal(
            IRenderContext renderContext, 
            PageArea pageArea)
        {
            var layout = Element as ILayout;
            if (ReferenceEquals(layout, null)) return WriteResult.Continue();

            return layout.WritePageArea(renderContext, pageArea, WriteZone);
        }

        private IWriteResult WriteZone(
            IRenderContext renderContext,
            PageArea pageArea,
            string zoneName)
        {
            if (_pageLayoutZones.TryGetValue(zoneName, out PageRegionElement pageLayoutZone))
                return pageLayoutZone.WritePageArea(renderContext, pageArea);

            return WriteResult.Continue();
        }
    }
}
