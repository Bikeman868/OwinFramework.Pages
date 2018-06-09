using System;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Facilities.Extensions;

namespace OwinFramework.Pages.Html.Runtime
{
    internal class RenderContext: IRenderContext, IDisposable
    {
        public IOwinContext OwinContext { get; private set; }
        public IHtmlWriter Html { get; private set; }

        public RenderContext(
            IHtmlWriter htmlWriter)
        {
            Html = htmlWriter;
        }

        public IRenderContext Initialize(IOwinContext context)
        {
            OwinContext = context;
            return this;
        }

        public void Dispose()
        {
            Html.Dispose();
        }
    }
}
