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

        private readonly IPageDependenciesFactory _dependenciesFactory;

        protected Page(IPageDependenciesFactory dependenciesFactory)
        {
            _dependenciesFactory = dependenciesFactory;
        }

        /// <summary>
        /// Override this method to completely takeover how the page is produced
        /// </summary>
        public virtual Task Run(IOwinContext owinContext)
        {
            var dependencies = _dependenciesFactory.Create(owinContext);
            var html = dependencies.RenderContext.Html;
            var data = dependencies.DataContext;
            var context = dependencies.RenderContext;

            owinContext.Response.ContentType = "text/html";

            try
            {
                html.WriteOpenTag("html");
                html.WriteOpenTag("head");

                html.WriteOpenTag("title");
                WriteTitle(context, data);
                html.WriteCloseTag("title");

                WriteHead(context, data);
                html.WriteCloseTag("head");

                if (string.IsNullOrEmpty(BodyClassNames))
                    html.WriteOpenTag("body");
                else
                    html.WriteOpenTag("body", "class", BodyClassNames);
                WriteHtml(context, data);
                html.WriteCloseTag("body");

                WriteInitializationScript(context, data);

                html.WriteCloseTag("html");
            }
            catch
            {
                dependencies.Dispose();
                throw;
            }

            return Task.Factory.StartNew(() =>
                {
                    html.ToResponse(owinContext);
                    dependencies.Dispose();
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
