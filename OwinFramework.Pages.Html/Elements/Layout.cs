using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Elements
{
    /// <summary>
    /// Base implementation of ILayout. Applications inherit from this olass 
    /// to insulate their code from any future additions to the ILayout interface
    /// </summary>
    public class Layout : Element, ILayout
    {
        public override ElementType ElementType { get { return ElementType.Layout; } }

        private VisualElement[] _visualElements;

        protected IThreadSafeDictionary<string, IRegion> RegionsByZoneName;
        protected IThreadSafeDictionary<string, IElement> ElementsByZoneName;

        public Layout(ILayoutDependenciesFactory dependencies)
            : base(dependencies.DataConsumerFactory)
        {
            // DO NOT change the method signature of this constructor as
            // this would break all layouts in all applications that use
            // this framework!!

            RegionsByZoneName = dependencies.DictionaryFactory.Create<string, IRegion>(StringComparer.InvariantCultureIgnoreCase);
            ElementsByZoneName = dependencies.DictionaryFactory.Create<string, IElement>(StringComparer.InvariantCultureIgnoreCase);
        }

        protected override T PopulateDebugInfo<T>(DebugInfo debugInfo, int parentDepth, int childDepth)
        {
            var debugLayout = debugInfo as DebugLayout ?? new DebugLayout();

            debugLayout.Children = RegionsByZoneName.Select(
                kvp => new DebugLayoutRegion
                {
                    Name = kvp.Key,
                    Instance = kvp.Value,
                    Region = kvp.Value.GetDebugInfo<DebugRegion>()
                })
                .Cast<DebugInfo>()
                .ToList();

            return base.PopulateDebugInfo<T>(debugLayout, parentDepth, childDepth);
        }

        public void PopulateZone(string zoneName, IRegion region)
        {
            RegionsByZoneName[zoneName] = region;
            PopulateElement(zoneName, region.Content);
        }

        public IEnumerable<string> GetZoneNames()
        {
            return RegionsByZoneName.Keys;
        }

        public void PopulateElement(string zoneName, IElement element)
        {
            ElementsByZoneName[zoneName] = element;
        }

        public IRegion GetRegion(string zoneName)
        {
            IRegion region;
            if (!RegionsByZoneName.TryGetValue(zoneName, out region))
                throw new Exception("Layout does not have a '" + zoneName + "' region");

            return region;
        }

        public IElement GetElement(string zoneName)
        {
            IElement element;
            if (!ElementsByZoneName.TryGetValue(zoneName, out element))
                throw new Exception("Layout does not have a '" + zoneName + "' region");

            return element;
        }

        public void AddVisualElement(Action<IHtmlWriter> writeAction, string comment)
        {
            var staticHtml = new StaticHtmlElement { WriteAction = writeAction, Comment = comment };
            var visualElement = new VisualElement { StaticHtml = staticHtml };
            Add(visualElement);
        }

        public void AddZoneVisualElement(string zoneName)
        {
            var visualElement = new VisualElement { ZoneName = zoneName };
            Add(visualElement);
        }

        private void Add(VisualElement visualElement)
        {
            if (_visualElements == null)
            {
                _visualElements = new[] { visualElement };
            }
            else
            {
                // This looks inefficient but only happens during initial configuration.
                // Making the visual elements collection an array avoids the need for locking
                // when rendering pages
                var list = _visualElements.ToList();
                list.Add(visualElement);
                _visualElements = list.ToArray();
            }
        }

        public virtual IWriteResult WritePageArea(
            IRenderContext context, 
            PageArea pageArea, 
            Func<IRenderContext, PageArea, string, IWriteResult> childWriter)
        {
            var result = WriteResult.Continue();

#if TRACE
            context.Trace(() => ToString() + " writing layout body");
            context.TraceIndent();
#endif

            if (context.IncludeComments)
                context.Html.WriteComment("layout " + this.FullyQualifiedName());

            for (var i = 0; i < _visualElements.Length; i++)
            {
                var visualElement = _visualElements[i];

                if (pageArea == PageArea.Body && !ReferenceEquals(visualElement.StaticHtml, null))
                    visualElement.StaticHtml.WriteAction(context.Html);

                if (!ReferenceEquals(visualElement.ZoneName, null))
                    result.Add(childWriter(context, pageArea, visualElement.ZoneName));
            }

#if TRACE
            context.TraceOutdent();
#endif
            return result;
        }


        private class VisualElement
        {
            public string ZoneName;
            public StaticHtmlElement StaticHtml;
        }
    }
}
