using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Runtime.Internal
{
    internal class BuiltLayout: Layout
    {
        private List<Func<Func<string, IElement>, IElement>> _visualElements;
        private Dictionary<int, int> _visualElementMapping;
        private List<string> _regionNameOrder;

        public BuiltLayout(ILayoutDependenciesFactory layoutDependenciesFactory)
            : base(layoutDependenciesFactory)
        { }

        public void AddVisualElement(Action<IHtmlWriter> writeAction, string comment)
        {
            if (_visualElements == null)
                _visualElements = new List<Func<Func<string, IElement>, IElement>>();

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
                _visualElements = new List<Func<Func<string, IElement>, IElement>>();

            _visualElements.Add(f => f(regionName));

            if (_visualElementMapping == null)
                _visualElementMapping = new Dictionary<int, int>();

            _visualElementMapping[_regionNameOrder.Count - 1] = _visualElements.Count - 1;
        }

        public override IWriteResult WriteHtml(IRenderContext renderContext, IDataContext dataContext, bool includeChildren)
        {
            return WriteHtml(
                renderContext, 
                dataContext, 
                includeChildren 
                ? (Func<string, IRegion>)(regionName => Content[regionName])
                : regionName => (IRegion)null);
            /*
            var result = WriteResult.Continue();

            if (renderContext.IncludeComments)
            {
                renderContext.Html.WriteComment(
                    (string.IsNullOrEmpty(Name) ? "nnnamed" : Name) +
                    (Package == null ? " layout" : " layout from " + Package.Name + " package"));

                var reverseMapping = new Dictionary<int, int>();
                for (var i = 0; i < _regions.Count; i++)
                {
                    var visualElementIndex = _visualElementMapping[i];
                    reverseMapping[visualElementIndex] = i;
                }

                for (var i = 0; i < _visualElements.Count; i++)
                {
                    if (reverseMapping.ContainsKey(i))
                    {
                        var regionIndex = reverseMapping[i];
                        renderContext.Html.WriteComment("layout '" + _regionNameOrder[regionIndex] + "' region");
                    }
                    result.Add(_visualElements[i].WriteHtml(renderContext, dataContext));
                }
            }
            else
            {
                foreach (var element in _visualElements)
                    result.Add(element.WriteHtml(renderContext, dataContext));
            }
            return result;
            */
        }

        public override IWriteResult WriteHtml(
            IRenderContext renderContext, 
            IDataContext dataContext, 
            Func<string, IRegion> contentFunc)
        {
            var result = WriteResult.Continue();

            foreach (var elementFunction in _visualElements)
            {
                var element = elementFunction(contentFunc);
                result.Add(element.WriteHtml(renderContext, dataContext));
            }

            return result;
        }
    }
}
