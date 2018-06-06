using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.BaseClasses
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
        /// Override this method to completely takeover how the page is produced
        /// </summary>
        public virtual Task Run(IOwinContext context)
        {
            context.Response.ContentType = "text/html";
            IRenderContext renderContext = null;
            IDataContext dataContext = null;

            var html = new StringBuilder();
            using (var writer = new StringWriter(html))
            {
                writer.Write("<html>");
                writer.Write("<head>");

                writer.Write("<title>");
                WriteTitle(renderContext, dataContext, writer);
                writer.Write("</title>");

                WriteHead(renderContext, dataContext, writer);
                writer.Write("</head>");

                writer.Write("<body>");
                WriteHtml(renderContext, dataContext, writer);
                writer.Write("</body>");

                WriteInitializationScript(renderContext, dataContext, writer);
                writer.Write("</html>");
            }

            return context.Response.WriteAsync(html.ToString());
        }
    }
}
