using System;
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
            IPageData initializationData)
            : base(dependencies, parent, region, initializationData)
        {
            _dataScopeProvider = initializationData.ScopeProvider.CreateInstance();
            _dataScopeProvider.Initialize(initializationData.ScopeProvider);

            initializationData.Push();
            initializationData.ScopeProvider = _dataScopeProvider;

            content = content ?? region.Content;
            var layout = content as ILayout;
            var component = content as IComponent;

            if (layout != null)
            {
                var pageLayout = new PageLayout(dependencies, this, layout, null, initializationData);
                _writeContent = pageLayout.WritePageArea;
                Children = new PageElement[] { pageLayout };
            }
            else if (component != null)
            {
                var pageLayout = new PageComponent(dependencies, this, component, initializationData);
                _writeContent = pageLayout.WritePageArea;
                Children = new PageElement[] { pageLayout };
            }
            else
            {
                Children = null;
                _writeContent = (rc, dc, pa) => WriteResult.Continue();
            }

            initializationData.Pop();
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
