using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Elements
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

        private List<Func<Func<string, IRegion>, IElement>> _visualElements;
        private Dictionary<int, int> _visualElementMapping;
        private List<string> _regionNameOrder;

        protected IThreadSafeDictionary<string, IRegion> RegionsByName;

        public Layout(ILayoutDependenciesFactory dependencies)
            : base(dependencies.DataConsumerFactory)
        {
            // DO NOT change the method signature of this constructor as
            // this would break all layouts in all applications that use
            // this framework!!

            _layoutDependenciesFactory = dependencies;
            RegionsByName = dependencies.DictionaryFactory.Create<string, IRegion>(StringComparer.InvariantCultureIgnoreCase);
        }

        protected override DebugInfo PopulateDebugInfo(DebugInfo debugInfo)
        {
            var debugLayout = debugInfo as DebugLayout ?? new DebugLayout();

            debugLayout.Regions = RegionsByName.Select(kvp =>
                new DebugLayoutRegion
                {
                    Name = kvp.Key,
                    Region = kvp.Value == null ? null : (DebugRegion)kvp.Value.GetDebugInfo()
                }).ToList();

            return base.PopulateDebugInfo(debugLayout);
        }

        public IRegion GetRegion(string regionName)
        {
            IRegion region;
            if (!RegionsByName.TryGetValue(regionName, out region))
                throw new Exception("Layout does not have a '" + regionName + "' region");

            return region;
        }

        public void AddVisualElement(Action<IHtmlWriter> writeAction, string comment)
        {
            if (_visualElements == null)
                _visualElements = new List<Func<Func<string, IRegion>, IElement>>();

            var staticElement = new StaticHtmlElement { WriteAction = writeAction, Comment = comment };
            _visualElements.Add(f => staticElement);
        }

        public void AddRegionVisualElement(string regionName)
        {
            if (_regionNameOrder == null)
                _regionNameOrder = new List<string>();

            _regionNameOrder.Add(regionName);

            if (_visualElements == null)
                _visualElements = new List<Func<Func<string, IRegion>, IElement>>();

            _visualElements.Add(f => f(regionName));

            if (_visualElementMapping == null)
                _visualElementMapping = new Dictionary<int, int>();

            _visualElementMapping[_regionNameOrder.Count - 1] = _visualElements.Count - 1;
        }

        public void SetRegionInstance(string regionName, IRegion region)
        {
            RegionsByName[regionName] = region;
        }

        public void Populate(string regionName, IElement element)
        {
            var region = GetRegion(regionName);
            var regionInstance = region.CreateInstance(element);
            SetRegionInstance(regionName, regionInstance);
        }

        public ILayout CreateInstance()
        {
            return new LayoutInstance(_layoutDependenciesFactory, this, _regionNameOrder);
        }

        public override IEnumerator<IElement> GetChildren()
        {
            return RegionsByName.Values.GetEnumerator();
        }

        public override IWriteResult WriteHtml(IRenderContext context, bool includeChildren)
        {
            return WriteHtml(
                context,
                includeChildren
                ? (Func<string, IRegion>)(regionName => RegionsByName[regionName])
                : regionName => (IRegion)null);
        }

        public virtual IWriteResult WriteHtml(
            IRenderContext context,
            Func<string, IRegion> regionLookup)
        {
            if (ReferenceEquals(regionLookup, null))
                regionLookup = regionName => null;

            var result = WriteResult.Continue();

            if (context.IncludeComments)
            {
                context.Trace(() => ToString() + " writing layout body including comments");
                context.TraceIndent();

                var layoutName = 
                    (string.IsNullOrEmpty(Name) ? "unnamed layout" : "'" + Name + "' layout") +
                    (Package == null ? string.Empty : " from the '" + Package.Name + "' package");
                context.Html.WriteComment(layoutName);

                var reverseMapping = new Dictionary<int, int>();
                for (var i = 0; i < _regionNameOrder.Count; i++)
                {
                    var visualElementIndex = _visualElementMapping[i];
                    reverseMapping[visualElementIndex] = i;
                }

                for (var i = 0; i < _visualElements.Count; i++)
                {
                    if (reverseMapping.ContainsKey(i))
                    {
                        var regionIndex = reverseMapping[i];
                        context.Html.WriteComment("'" + _regionNameOrder[regionIndex] + "' region of the " + layoutName);
                    }
                    var element = _visualElements[i](regionLookup);
                    if (element != null)
                        result.Add(element.WriteHtml(context));
                }

                context.TraceOutdent();
            }
            else
            {
                context.Trace(() => ToString() + " writing layout body without comments");
                context.TraceIndent();

                foreach (var elementFunction in _visualElements)
                {
                    var element = elementFunction(regionLookup);
                    if (element != null)
                        result.Add(element.WriteHtml(context));
                }
                
                context.TraceOutdent();
            }

            return result;
        }
    }
}
