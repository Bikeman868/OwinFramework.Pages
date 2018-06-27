using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace Sample1.SamplePages
{
    [IsPage]
    [RequiresPermission("administrator")]
    [Route("/page3", Methods.Get)]
    [Description("<p>This is an example of how to add a full custom page that is discovered and registered automatically by the fluent builder</p>")]
    [Option(OptionType.Method, "GET", "<p>Returns the html for this self registering page</p>")]
    [Example("<a href='/page3'>/page3</a>")]
    internal class SelfRegisteringPage : IPage
    {
        public string Name { get; set; }
        public IPackage Package { get; set; }
        string IRunable.RequiredPermission { get { return null; } set { } }
        bool IRunable.AllowAnonymous { get { return true; } set { } }
        Func<IOwinContext, bool> IRunable.AuthenticationFunc { get { return null; } }

        public void Initialize()
        {
        }

        DebugInfo IRunable.GetDebugInfo() { return GetDebugInfo(); }

        public DebugPage GetDebugInfo()
        {
            return new DebugPage
            {
                Name = Name,
                Instance = this
            };
        }

        Task IRunable.Run(IOwinContext context)
        {
            context.Response.ContentType = "text/html";
            return context.Response.WriteAsync("<html><head><title>Self registered</title></head><body>This is a self registered page with routing defined by attributes</body></html>");
        }
    }
}