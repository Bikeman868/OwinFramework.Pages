using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace Sample1.SamplePages
{
    [Description("<p>This is an example of how to add a full custom page that directly implements IPage</p>")]
    [Option(OptionType.Method, "GET", "<p>Returns the html for this custom page</p>")]
    [Option(OptionType.Header, "Accept", "text/html")]
    [Example("<a href='/pages/anything.html'>/pages/anything.html</a>")]
    internal class FullCustomPage : IPage
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

        Task IRunable.Run(IOwinContext context, Action<IOwinContext, Func<string>> trace)
        {
            context.Response.ContentType = "text/html";
            return context.Response.WriteAsync("<html><head><title>Full custom</title></head><body>This is a fully custom page</body></html>");
        }
    }
}