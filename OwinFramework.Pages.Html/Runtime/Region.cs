using System.Collections.Generic;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Builders;
using OwinFramework.Pages.Html.Runtime.Internal;

namespace OwinFramework.Pages.Html.Runtime
{
    /// <summary>
    /// Base implementation of IRegion. Inheriting from this class will insulate you
    /// from any future additions to the IRegion interface.
    /// You can also use this class directly but it provides only minimal region 
    /// functionallity
    /// </summary>
    public class Region : Element, IRegion
    {
        private readonly IRegionDependenciesFactory _regionDependenciesFactory;
        public override ElementType ElementType { get { return ElementType.Region; } }
        public bool IsClone { get { return false; } }

        protected IElement Content;

        /// <summary>
        /// Do not change this constructor signature, it will break application
        /// classes that inherit from this class. Add dependencies to
        /// IRegionDependenciesFactory and IRegionDependencies
        /// </summary>
        public Region(IRegionDependenciesFactory regionDependenciesFactory)
        {
            _regionDependenciesFactory = regionDependenciesFactory;
        }

        public virtual void Populate(IElement content)
        {
            Content = content;
        }

        public IRegion Clone(IElement content)
        {
            return new ClonedRegion(_regionDependenciesFactory, this, content);
        }

        public override IEnumerator<IElement> GetChildren()
        {
            return Content == null 
                ? null 
                : Content.AsEnumerable().GetEnumerator();
        }

        public virtual IWriteResult WriteHtml(
            IRenderContext renderContext,
            IDataContext dataContext, 
            IElement content)
        {
            return content == null 
                ? WriteResult.Continue() 
                : content.WriteHtml(renderContext, dataContext);
        }
    }
}
