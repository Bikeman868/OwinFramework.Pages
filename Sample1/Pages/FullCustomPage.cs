using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace Sample1.Pages
{
    [Description(Html = "<p>This is an example of how to add a full custom page</p>")]
    [Option(OptionType = OptionType.Method, Name = "GET", Html = "<p>Returns the html for this custom page</p>")]
    internal class FullCustomPage: IPage
    {
        string IRunable.RequiredPermission { get { return null; } }
        bool IRunable.AllowAnonymous{get { return true; }}
        Func<IOwinContext, bool> IRunable.AuthenticationFunc { get { return null; } }

        Task IRunable.Run(IOwinContext context)
        {
            context.Response.ContentType = "text/html";
            return context.Response.WriteAsync("<html><head><title>Full custom</title></head><body>This is a fully custom page</body></html>");
        }
    }
}