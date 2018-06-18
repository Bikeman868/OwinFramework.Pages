using System;
using System.Linq;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Builders
{
    internal class ClonedLayout: ClonedElement<ILayout>, ILayout
    {
        public ElementType ElementType { get { return ElementType.Region; } }

        private readonly ILayoutDependenciesFactory _layoutDependencies;
        private readonly IThreadSafeDictionary<string, IRegion> _content;

        public ClonedLayout(
            ILayoutDependenciesFactory layoutDependencies,
            ILayout parent,
            IEnumerable<string> regionNames)
            : base(parent)
        {
            _layoutDependencies = layoutDependencies;
            _content = layoutDependencies.DictionaryFactory.Create<string, IRegion>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var regionName in regionNames)
                _content.Add(regionName, null);
        }

        public IRegion GetRegion(string regionName)
        {
            IRegion region;
            return _content.TryGetValue(regionName, out region) 
                ? region 
                : Parent.GetRegion(regionName);
        }

        public void Populate(string regionName, IElement element)
        {
            IRegion region;
            if (!_content.TryGetValue(regionName, out region))
                throw new Exception("Layout does not have a '" + regionName + "' region");

            if (region == null)
            {
                _content[regionName] = Parent.GetRegion(regionName).Clone(element);
            }
            else if (region.IsClone)
            {
                region.Populate(element);
            }
            else
            {
                _content[regionName] = region.Clone(element);
            }
        }

        public ILayout Clone()
        {
            using (var regionNames = _content.KeysLocked)
                return new ClonedLayout(_layoutDependencies, Parent, regionNames);
        }

        public override IEnumerator<IElement> GetChildren()
        {
            return _content.Select(e => e.Value ?? Parent.GetRegion(e.Key)).GetEnumerator();
        }

        public IWriteResult WriteHtml(IRenderContext renderContext, IDataContext dataContext, bool includeChildren)
        {
            return Parent.WriteHtml(
                renderContext,
                dataContext,
                includeChildren 
                    ? r => 
                    {
                        IRegion element;
                        _content.TryGetValue(r, out element);
                        return element;
                    }
                    : (Func<string, IRegion>)null);
        }

        public IWriteResult WriteHtml(IRenderContext renderContext, IDataContext dataContext, Func<string, IRegion> contentFunc)
        {
            return Parent.WriteHtml(
                renderContext,
                dataContext,
                r =>
                {
                    var element = contentFunc(r);
                    if (element == null)
                        _content.TryGetValue(r, out element);
                    return element;
                });
        }
    }
}
