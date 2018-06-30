using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Builders;

namespace OwinFramework.Pages.Html.Runtime
{
    /// <summary>
    /// Base implementation of ILayout. Inheriting from this olass will insulate you
    /// from any future additions to the ILayout interface
    /// You can also use this class directly but it provides only minimal region 
    /// functionallity
    /// </summary>
    public class Layout : Element, ILayout
    {
        private readonly ILayoutDependenciesFactory _layoutDependenciesFactory;
        public override ElementType ElementType { get { return ElementType.Layout; } }
        public bool IsInstance { get { return false; } }

        protected IThreadSafeDictionary<string, IRegion> Content;

        public Layout(ILayoutDependenciesFactory layoutDependenciesFactory)
        {
            // DO NOT change the method signature of this constructor as
            // this would break all layouts in all applications that use
            // this framework!!

            _layoutDependenciesFactory = layoutDependenciesFactory;
            Content = layoutDependenciesFactory.DictionaryFactory.Create<string, IRegion>(StringComparer.InvariantCultureIgnoreCase);
        }

        DebugElement IElement.GetDebugInfo() { return GetDebugInfo(); }

        public DebugLayout GetDebugInfo()
        {
            var debugInfo = new DebugLayout
            {
                Regions = Content.Select(kvp => 
                    new DebugLayoutRegion 
                    { 
                        Name = kvp.Key,
                        Region = kvp.Value == null ? null : kvp.Value.GetDebugInfo()
                    }).ToList()
            };
            PopulateDebugInfo(debugInfo);
            return debugInfo;
        }

        public IRegion GetRegion(string regionName)
        {
            IRegion region;
            if (!Content.TryGetValue(regionName, out region))
                throw new Exception("Layout doe not have a '" + regionName + "' region");

            return region;
        }

        public void Populate(string regionName, IElement element)
        {
            var region = GetRegion(regionName);

            if (region != null)
                region.Populate(element);
        }

        public ILayout CreateInstance()
        {
            using (var regionNames = Content.KeysLocked)
                return new LayoutInstance(_layoutDependenciesFactory, this, regionNames);
        }

        public override IEnumerator<IElement> GetChildren()
        {
            return Content.Values.Where(r => r != null).GetEnumerator();
        }

        public virtual IWriteResult WriteHtml(
            IRenderContext context, 
            Func<string, IRegion> regionLookup)
        {
            var result = WriteResult.Continue();
            if (regionLookup == null || Content.Count == 0) return result;

            using (var regionNames = Content.KeysLocked)
            {
                foreach(var regionName in regionNames)
                {
                    var region = regionLookup(regionName) ?? Content[regionName];
                    if (region != null)
                        result.Add(region.WriteHtml(context));
                }
            }
            return result;
        }
    }
}
