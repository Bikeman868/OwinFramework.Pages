using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Runtime.Internal
{
    public class ClonedRegion : ClonedElement<IRegion>, IRegion
    {
        public ElementType ElementType { get { return ElementType.Region; } }

        private readonly IRegionDependenciesFactory _dependenciesFactory;
        private IElement _content;

        public ClonedRegion(
            IRegionDependenciesFactory dependenciesFactory, 
            IRegion parent, 
            IElement content)
            : base(parent)
        {
            _dependenciesFactory = dependenciesFactory;
            _content = content;
        }

        public void Populate(IElement content)
        {
            _content = content;
        }

        public IRegion Clone(IElement content)
        {
            return new ClonedRegion(_dependenciesFactory, Parent, content);
        }

        public override IEnumerator<IElement> GetChildren()
        {
            return _content == null ? null : _content.AsEnumerable().GetEnumerator();
        }

        public IWriteResult WriteHtml(IRenderContext renderContext, IDataContext dataContext, bool includeChildren)
        {
            return Parent.WriteHtml(renderContext, dataContext, includeChildren ? _content : null);
        }

        public IWriteResult WriteHtml(IRenderContext renderContext, IDataContext dataContext, IElement content)
        {
            return Parent.WriteHtml(renderContext, dataContext, content);
        }
    }
}
