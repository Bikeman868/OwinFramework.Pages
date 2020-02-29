using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Runtime;
using System.Collections.Generic;

namespace OwinFramework.Pages.Html.Elements
{
    /// <summary>
    /// This is the runtime version of a component and is specific to an instance of 
    /// the component on a specific page
    /// </summary>
    internal class PageComponent : PageElement
    {
        public PageComponent(
            PageElementDependencies dependencies,
            PageElement parent,
            IComponent component, 
            IPageData pageData)
            : base(dependencies, parent, component, pageData)
        {
            pageData.BeginAddElement(Element);
            pageData.EndAddElement(Element);
        }

        public IElement Component => Element;

        protected override T PopulateDebugInfo<T>(DebugInfo debugInfo, int parentDepth, int childDepth)
        {
            var debugComponent = debugInfo as DebugComponent ?? new DebugComponent();

            return base.PopulateDebugInfo<T>(debugComponent, parentDepth, childDepth);
        }

        protected override IWriteResult WritePageAreaInternal(
            IRenderContext renderContext, 
            PageArea pageArea)
        {
            var component = Element as IComponent;
            if (ReferenceEquals(component, null)) return WriteResult.Continue();

            return component.WritePageArea(renderContext, pageArea);
        }
    }
}
