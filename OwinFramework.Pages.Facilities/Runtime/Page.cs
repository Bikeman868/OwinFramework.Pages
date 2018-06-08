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

        /// <summary>
        /// Override this method to completely takeover how the page is produced
        /// </summary>
        public virtual Task Run(IOwinContext context)
        {
            context.Response.ContentType = "text/html";
            IRenderContext renderContext = null;
            IDataContext dataContext = null;

            var writer = new HtmlWriter();
            try
            {
                writer.WriteOpenTag("html");
                writer.WriteOpenTag("head");

                writer.WriteOpenTag("title");
                WriteTitle(renderContext, dataContext, writer);
                writer.WriteCloseTag("title");

                WriteHead(renderContext, dataContext, writer);
                writer.WriteCloseTag("head");

                if (string.IsNullOrEmpty(BodyClassNames))
                {
                    writer.WriteOpenTag("body");
                }
                else
                {
                    writer.WriteOpenTag("body", "class", BodyClassNames);
                }
                WriteHtml(renderContext, dataContext, writer);
                writer.WriteCloseTag("body");

                WriteInitializationScript(renderContext, dataContext, writer);

                writer.WriteCloseTag("html");

                var task = new Task(() =>
                    {
                        writer.ToResponse(context);
                        writer.Dispose();
                    });
                task.Start();
                return task;
            }
            catch
            {
                writer.Dispose();
                throw;
            }
        }

        public override IWriteResult WriteStaticAssets(AssetType assetType, IHtmlWriter writer)
        {
            return Layout != null ? Layout.WriteStaticAssets(assetType, writer) : null;
        }

        public override IWriteResult WriteDynamicAssets(IRenderContext renderContext, IDataContext dataContext, AssetType assetType, IHtmlWriter writer)
        {
            return Layout != null ? Layout.WriteDynamicAssets(renderContext, dataContext, assetType, writer) : null;
        }

        public override IWriteResult WriteInitializationScript(IRenderContext renderContext, IDataContext dataContext, IHtmlWriter writer)
        {
            return Layout != null ? Layout.WriteInitializationScript(renderContext, dataContext, writer) : null;
        }

        public override IWriteResult WriteTitle(IRenderContext renderContext, IDataContext dataContext, IHtmlWriter writer)
        {
            return Layout != null ? Layout.WriteTitle(renderContext, dataContext, writer) : null;
        }

        public override IWriteResult WriteHead(IRenderContext renderContext, IDataContext dataContext, IHtmlWriter writer)
        {
            return Layout != null ? Layout.WriteHead(renderContext, dataContext, writer) : null;
        }

        public override IWriteResult WriteHtml(IRenderContext renderContext, IDataContext dataContext, IHtmlWriter writer)
        {
            return Layout != null ? Layout.WriteHtml(renderContext, dataContext, writer) : null;
        }
    }
}
