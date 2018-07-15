using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Elements
{
    internal class LayoutInstance: ElementInstance<ILayout>, ILayout
    {
        public override ElementType ElementType { get { return ElementType.Region; } }

        private readonly ILayoutDependenciesFactory _layoutDependencies;
        private readonly IThreadSafeDictionary<string, IRegion> _regionsByName;

        public LayoutInstance(
            ILayoutDependenciesFactory layoutDependencies,
            ILayout parent,
            IEnumerable<string> regionNames)
            : base(layoutDependencies.DataConsumerFactory, parent)
        {
            _layoutDependencies = layoutDependencies;
            _regionsByName = layoutDependencies.DictionaryFactory.Create<string, IRegion>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var regionName in regionNames)
            {
                var parentRegion = Parent.GetRegion(regionName);
                var regionInstance = parentRegion.CreateInstance(null);
                _regionsByName.Add(regionName, regionInstance);
            }
        }

        protected override DebugInfo PopulateDebugInfo(DebugInfo debugInfo)
        {
            var parentDebugInfo = (DebugLayout)Parent.GetDebugInfo();

            var debugLayout = debugInfo as DebugLayout ?? new DebugLayout();

            debugLayout.Type = "Instance of layout";
            debugLayout.InstanceOf = parentDebugInfo;
            debugLayout.Regions = _regionsByName
                .Select(kvp => new DebugLayoutRegion
                    {
                        Name = kvp.Key,
                        Region = kvp.Value == null ? null : (DebugRegion)kvp.Value.GetDebugInfo()
                    })
                .ToList();

            return base.PopulateDebugInfo(debugLayout);
        }

        public IRegion GetRegion(string regionName)
        {
            IRegion region;
            if (_regionsByName.TryGetValue(regionName, out region))
                return region;

            throw new Exception("Layout does not have a '" + regionName + "' region");
        }

        public void Populate(string regionName, IElement element)
        {
            var region = GetRegion(regionName);
            region.Populate(element);
        }

        public ILayout CreateInstance()
        {
            using (var regionNames = _regionsByName.KeysLocked)
                return new LayoutInstance(_layoutDependencies, this, regionNames);
        }

        public override IEnumerator<IElement> GetChildren()
        {
            return _regionsByName.Select(e => e.Value).Where(r => r != null).GetEnumerator();
        }

        public override IWriteResult WriteHtml(IRenderContext context, bool includeChildren)
        {
            return Parent.WriteHtml(
                context,
                includeChildren 
                    ? GetRegion
                    : (Func<string, IRegion>)null);
        }

        public IWriteResult WriteHtml(IRenderContext context, Func<string, IRegion> regionLookup)
        {
            return Parent.WriteHtml(
                context,
                regionName =>
                {
                    if (regionLookup == null) return null;
                    var region = regionLookup(regionName);
                    if (region != null) return region;
                    return GetRegion(regionName);
                });
        }
    }
}
