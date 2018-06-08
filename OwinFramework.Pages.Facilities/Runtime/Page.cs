using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Facilities.Runtime
{
    /// <summary>
    /// Base implementation of IPage. Inheriting from this olass will insulate you
    /// from any additions to the IPage interface
    /// </summary>
    public abstract class Page: Element, IPage
    {
        /// <summary>
        /// Returns the name of the permission that the user must have to view this page
        /// </summary>
        public virtual string RequiredPermission { get { return null; } }

        /// <summary>
        /// Return false if anonymouse users are not permitted to view this page
        /// </summary>
        public virtual bool AllowAnonymous { get{return true; } }

        /// <summary>
        /// Return a custom authentication check
        /// </summary>
        public virtual Func<IOwinContext, bool> AuthenticationFunc { get { return null; } }

        /// <summary>
        /// Defines the layout of this page
        /// </summary>
        public ILayout Layout { get; set; }

        /// <summary>
        /// The names of the css class to attach to the body element
        /// </summary>
        public string BodyClassNames { get; set; }

        private readonly IPageDependencies _dependencies;

        protected Page(IPageDependencies dependencies)
        {
            _dependencies = dependencies;
        }

        /// <summary>
        /// Override this method to completely takeover how the page is produced
        /// </summary>
        public virtual Task Run(IOwinContext context)
        {
            _dependencies.Initialize(context);

            context.Response.ContentType = "text/html";

            var html = _dependencies.RenderContext.Html;

            try
            {
                html.WriteOpenTag("html");
                html.WriteOpenTag("head");

                html.WriteOpenTag("title");
                WriteTitle(_dependencies.RenderContext, _dependencies.DataContext);
                html.WriteCloseTag("title");

                WriteHead(_dependencies.RenderContext, _dependencies.DataContext);
                html.WriteCloseTag("head");

                if (string.IsNullOrEmpty(BodyClassNames))
                    html.WriteOpenTag("body");
                else
                    html.WriteOpenTag("body", "class", BodyClassNames);
                WriteHtml(_dependencies.RenderContext, _dependencies.DataContext);
                html.WriteCloseTag("body");

                WriteInitializationScript(_dependencies.RenderContext, _dependencies.DataContext);

                html.WriteCloseTag("html");
            }
            catch
            {
                _dependencies.Dispose();
                throw;
            }

            return Task.Factory.StartNew(() =>
                {
                    html.ToResponse(context);
                    _dependencies.Dispose();
                });
        }

        public override IWriteResult WriteStaticAssets(AssetType assetType, IHtmlWriter writer)
        {
            return Layout != null ? Layout.WriteStaticAssets(assetType, writer) : WriteResult.Continue();
        }

        public override IWriteResult WriteDynamicAssets(IRenderContext renderContext, IDataContext dataContext, AssetType assetType)
        {
            return Layout != null ? Layout.WriteDynamicAssets(renderContext, dataContext, assetType) : WriteResult.Continue();
        }

        public override IWriteResult WriteInitializationScript(IRenderContext renderContext, IDataContext dataContext)
        {
            return Layout != null ? Layout.WriteInitializationScript(renderContext, dataContext) : WriteResult.Continue();
        }

        public override IWriteResult WriteTitle(IRenderContext renderContext, IDataContext dataContext)
        {
            return Layout != null ? Layout.WriteTitle(renderContext, dataContext) : WriteResult.Continue();
        }

        public override IWriteResult WriteHead(IRenderContext renderContext, IDataContext dataContext)
        {
            return Layout != null ? Layout.WriteHead(renderContext, dataContext) : WriteResult.Continue();
        }

        public override IWriteResult WriteHtml(IRenderContext renderContext, IDataContext dataContext)
        {
            return Layout != null ? Layout.WriteHtml(renderContext, dataContext) : WriteResult.Continue();
        }
    }
}
