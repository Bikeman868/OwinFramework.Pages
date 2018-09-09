using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Elements
{
    /// <summary>
    /// This class is responsible for rendering components onto a specific page
    /// in response to requests for that page
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
        }

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
