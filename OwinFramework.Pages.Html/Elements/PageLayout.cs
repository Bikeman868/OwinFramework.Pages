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
    /// This is the runtime version of a layout and is specific to an instance of 
    /// the layout on a specific page
    /// </summary>
    internal class PageLayout: PageElement
    {
        private readonly IThreadSafeDictionary<string, PageRegion> _zones;

        public PageLayout(
            PageElementDependencies dependencies,
            PageElement parent,
            ILayout layout, 
            IEnumerable<Tuple<string, IRegion, IElement>> regionElements,
            IPageData pageData)
            : base(dependencies, parent, layout, pageData)
        {
            pageData.BeginAddElement(Element);

            _zones = dependencies.DictionaryFactory.Create<string, PageRegion>(StringComparer.OrdinalIgnoreCase);

            var regionElementList = regionElements == null
                ? new List<Tuple<string, IRegion, IElement>>() 
                : regionElements.ToList();

            foreach(var zoneName in layout.GetZoneNames())
            {
                var name = zoneName;
                var regionElement = regionElementList.FirstOrDefault(
                    re => string.Equals(re.Item1, name, StringComparison.OrdinalIgnoreCase));

                var region = regionElement?.Item2 ?? layout.GetRegion(name);
                var element = regionElement?.Item3 ?? layout.GetElement(name);

                var pageRegion = new PageRegion(dependencies, this, region, element, pageData);
                _zones[zoneName] = pageRegion;
            }

            Children = _zones.Values.ToArray();

            pageData.EndAddElement(Element);
        }

        protected override T PopulateDebugInfo<T>(DebugInfo debugInfo, int parentDepth, int childDepth)
        {
            var debugLayout = debugInfo as DebugLayout ?? new DebugLayout();

            if (childDepth != 0)
            {
                debugLayout.Children = _zones
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
            if (_zones.TryGetValue(zoneName, out PageRegion pageZone))
                return pageZone.WritePageArea(renderContext, pageArea);

            return WriteResult.Continue();
        }
    }
}
