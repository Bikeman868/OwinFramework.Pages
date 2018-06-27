﻿using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Builders
{
    internal class BuiltLayout: Layout
    {
        private List<Func<Func<string, IRegion>, IElement>> _visualElements;
        private Dictionary<int, int> _visualElementMapping;
        private List<string> _regionNameOrder;

        public BuiltLayout(ILayoutDependenciesFactory layoutDependenciesFactory)
            : base(layoutDependenciesFactory)
        { }

        public void AddVisualElement(Action<IHtmlWriter> writeAction, string comment)
        {
            if (_visualElements == null)
                _visualElements = new List<Func<Func<string, IRegion>, IElement>>();

            var staticElement = new StaticHtmlElement { WriteAction = writeAction, Comment = comment };
            _visualElements.Add(f => staticElement);
        }

        public void AddRegion(string regionName, IRegion region, IElement element = null)
        {
            if (element != null)
                region = region.Clone(element);

            Content[regionName] = region;

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

        public override IWriteResult WriteHtml(IRenderContext context, bool includeChildren)
        {
            return WriteHtml(
                context, 
                includeChildren 
                ? (Func<string, IRegion>)(regionName => Content[regionName])
                : regionName => (IRegion)null);
        }

        public override IWriteResult WriteHtml(
            IRenderContext context, 
            Func<string, IRegion> regionLookup)
        {
            var result = WriteResult.Continue();

            if (context.IncludeComments)
            {
                context.Html.WriteComment(
                    (string.IsNullOrEmpty(Name) ? "unnamed" : Name) +
                    (Package == null ? " layout" : " layout from " + Package.Name + " package"));

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
                        context.Html.WriteComment("layout '" + _regionNameOrder[regionIndex] + "' region");
                    }
                    var element = _visualElements[i](regionLookup);
                    if (element != null)
                        result.Add(element.WriteHtml(context));
                }
            }
            else
            {
                foreach (var elementFunction in _visualElements)
                {
                    var element = elementFunction(regionLookup);
                    if (element != null)
                        result.Add(element.WriteHtml(context));
                }
            }

            return result;
        }
    }
}