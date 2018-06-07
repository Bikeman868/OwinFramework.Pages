using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Core.Runtime;

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
                writer.WriteLine();

                writer.WriteOpenTag("head");
                writer.WriteLine();
                writer.WriteOpenTag("title");
                WriteTitle(renderContext, dataContext, writer);
                writer.WriteCloseTag("title");
                writer.WriteLine();
                WriteHead(renderContext, dataContext, writer);
                writer.WriteCloseTag("head");
                writer.WriteLine();

                if (string.IsNullOrEmpty(BodyClassNames))
                {
                    writer.WriteOpenTag("body");
                }
                else
                {
                    writer.WriteOpenTag("body", "class", BodyClassNames);
                }
                writer.WriteLine();
                WriteHtml(renderContext, dataContext, writer);
                writer.WriteCloseTag("body");
                writer.WriteLine();

                WriteInitializationScript(renderContext, dataContext, writer);

                writer.WriteCloseTag("html");

                return Task.Factory.StartNew(() =>
                    {
                        writer.ToResponse(context);
                        writer.Dispose();
                    });
            }
            catch
            {
                writer.Dispose();
                throw;
            }
        }

        public override IWriteResult WriteStaticAssets(AssetType assetType, HtmlWriter writer)
        {
            return Layout != null ? Layout.WriteStaticAssets(assetType, writer) : null;
        }

        public override IWriteResult WriteDynamicAssets(IRenderContext renderContext, IDataContext dataContext, AssetType assetType, HtmlWriter writer)
        {
            return Layout != null ? Layout.WriteDynamicAssets(renderContext, dataContext, assetType, writer) : null;
        }

        public override IWriteResult WriteInitializationScript(IRenderContext renderContext, IDataContext dataContext, HtmlWriter writer)
        {
            return Layout != null ? Layout.WriteInitializationScript(renderContext, dataContext, writer) : null;
        }

        public override IWriteResult WriteTitle(IRenderContext renderContext, IDataContext dataContext, HtmlWriter writer)
        {
            return Layout != null ? Layout.WriteTitle(renderContext, dataContext, writer) : null;
        }

        public override IWriteResult WriteHead(IRenderContext renderContext, IDataContext dataContext, HtmlWriter writer)
        {
            return Layout != null ? Layout.WriteHead(renderContext, dataContext, writer) : null;
        }

        public override IWriteResult WriteHtml(IRenderContext renderContext, IDataContext dataContext, HtmlWriter writer)
        {
            return Layout != null ? Layout.WriteHtml(renderContext, dataContext, writer) : null;
        }
    }
}
