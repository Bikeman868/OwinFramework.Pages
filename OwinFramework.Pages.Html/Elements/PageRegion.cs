using System;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Elements
{
    internal class PageRegion : PageElement
    {
        private readonly IDataContextBuilder _dataContextBuilder;
        private readonly Func<IRenderContext, PageArea, IWriteResult> _writeContent;

        public PageRegion(
            PageElementDependencies dependencies,
            PageElement parent,
            IRegion region, 
            IElement content, 
            IPageData pageData)
            : base(dependencies, parent, region, pageData)
        {
            _dataContextBuilder = pageData.BeginAddElement(Element);

            content = content ?? region.Content;
            var layout = content as ILayout;
            var component = content as IComponent;

            if (layout != null)
            {
                var pageLayout = new PageLayout(dependencies, this, layout, null, pageData);
                _writeContent = pageLayout.WritePageArea;
                Children = new PageElement[] { pageLayout };
            }
            else if (component != null)
            {
                var pageComponent = new PageComponent(dependencies, this, component, pageData);
                _writeContent = pageComponent.WritePageArea;
                Children = new PageElement[] { pageComponent };
            }
            else
            {
                Children = null;
                _writeContent = (rc, pa) => WriteResult.Continue();
            }

            pageData.EndAddElement(Element);
        }

        protected override T PopulateDebugInfo<T>(DebugInfo debugInfo, int parentDepth, int childDepth)
        {
            var debugRegion = debugInfo as DebugRegion ?? new DebugRegion();

            debugRegion.Scope = _dataContextBuilder.GetDebugInfo<DebugDataScopeRules>();

            return base.PopulateDebugInfo<T>(debugRegion, parentDepth, childDepth);
        }

        protected override IWriteResult WritePageAreaInternal(IRenderContext renderContext, PageArea pageArea)
        {
            var data = renderContext.Data;
            renderContext.SelectDataContext(_dataContextBuilder.Id);

            var result = _writeContent(renderContext, pageArea);

            renderContext.Data = data;
            return result;
        }
    }
}
