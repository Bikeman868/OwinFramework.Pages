using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.BaseClasses;

namespace Sample1.Pages
{
    [Description("<p>This is an example of how to add a semi custom page that inherits from the base Page class</p>")]
    internal class SemiCustomPage : Page
    {
        public override Task Run(IOwinContext context)
        {
            context.Response.ContentType = "text/html";
            return context.Response.WriteAsync("<html><head><title>Semi custom</title></head><body>This is a semi custom page</body></html>");
        }
    }
}