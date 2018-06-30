using System;
using System.Linq;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Builders
{
    internal class LayoutInstance: ElementInstance<ILayout>, ILayout
    {
        public ElementType ElementType { get { return ElementType.Region; } }

        private readonly ILayoutDependenciesFactory _layoutDependencies;
        private readonly IThreadSafeDictionary<string, IRegion> _content;

        public LayoutInstance(
            ILayoutDependenciesFactory layoutDependencies,
            ILayout parent,
            IEnumerable<string> regionNames)
            : base(parent)
        {
            _layoutDependencies = layoutDependencies;
            _content = layoutDependencies.DictionaryFactory.Create<string, IRegion>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var regionName in regionNames)
                _content.Add(regionName, Parent.GetRegion(regionName).CreateInstance(null));
        }

        DebugElement IElement.GetDebugInfo() { return GetDebugInfo(); }

        public DebugLayout GetDebugInfo()
        {
            var parentDebugInfo = Parent.GetDebugInfo();

            var debugInfo = new DebugLayout
            {
                Type = "Layout instance",
                InstanceOf = parentDebugInfo,
                Regions = _content
                    .Select(kvp => new DebugLayoutRegion 
                    { 
                        Name = kvp.Key,
                        Region = kvp.Value == null ? null : kvp.Value.GetDebugInfo() 
                    })
                    .ToList()
            };
            PopulateDebugInfo(debugInfo);
            return debugInfo;
        }

        public IRegion GetRegion(string regionName)
        {
            IRegion region;
            if (_content.TryGetValue(regionName, out region))
                return region;

            throw new Exception("Layout does not have a '" + regionName + "' region");
        }

        public void Populate(string regionName, IElement element)
        {
            var region = GetRegion(regionName);

            if (region != null)
                region.Populate(element);
        }

        public ILayout CreateInstance()
        {
            using (var regionNames = _content.KeysLocked)
                return new LayoutInstance(_layoutDependencies, this, regionNames);
        }

        public override IEnumerator<IElement> GetChildren()
        {
            return _content.Select(e => e.Value).Where(r => r != null).GetEnumerator();
        }

        public IWriteResult WriteHtml(IRenderContext context, bool includeChildren)
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
