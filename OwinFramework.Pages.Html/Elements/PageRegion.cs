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
        private readonly IDataScopeProvider _dataScopeProvider;
        private readonly Func<IRenderContext, IDataContextBuilder, PageArea, IWriteResult> _writeContent;

        public PageRegion(
            PageElementDependencies dependencies,
            PageElement parent,
            IRegion region, 
            IElement content, 
            IPageData pageData)
            : base(dependencies, parent, region, pageData)
        {
            _dataScopeProvider = pageData.DataContextBuilder.CreateInstance();
            _dataScopeProvider.Initialize(pageData.DataContextBuilder);

            pageData.Push();
            pageData.DataContextBuilder = _dataScopeProvider;

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
                var pageLayout = new PageComponent(dependencies, this, component, pageData);
                _writeContent = pageLayout.WritePageArea;
                Children = new PageElement[] { pageLayout };
            }
            else
            {
                Children = null;
                _writeContent = (rc, dc, pa) => WriteResult.Continue();
            }

            pageData.Pop();
        }

        protected override DebugInfo PopulateDebugInfo(DebugInfo debugInfo, int parentDepth, int childDepth)
        {
            var debugRegion = debugInfo as DebugRegion ?? new DebugRegion();
            return base.PopulateDebugInfo(debugRegion, parentDepth, childDepth);
        }

        protected override IWriteResult WritePageAreaInternal(IRenderContext renderContext, IDataContextBuilder dataContextBuilder, PageArea pageArea)
        {
            var data = renderContext.Data;
            renderContext.SelectDataContext(_dataScopeProvider.Id);

            var result = _writeContent(renderContext, dataContextBuilder, pageArea);

            renderContext.Data = data;
            return result;
        }

        public override void BuildDataContext(DataContextBuilder dataContextBuilder)
        {
            dataContextBuilder.Push(_dataScopeProvider);

            base.BuildDataContext(dataContextBuilder);

            dataContextBuilder.Pop();
        }
    }
}
