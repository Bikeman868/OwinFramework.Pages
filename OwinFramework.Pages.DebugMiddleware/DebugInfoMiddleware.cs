using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Owin;
using Newtonsoft.Json;
using OwinFramework.Builder;
using OwinFramework.Interfaces.Builder;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.DebugMiddleware.SvgDrawing;

namespace OwinFramework.Pages.DebugMiddleware
{
    /// <summary>
    /// This middleware extracts and returns debug information when ?debug=true is added to the URL
    /// </summary>
    public class DebugInfoMiddleware: IMiddleware<object>
    {
        private readonly IRequestRouter _requestRouter;
        private readonly IHtmlWriterFactory _htmlWriterFactory;
        private readonly IRenderContextFactory _renderContextFactory;
        private readonly IList<IDependency> _dependencies = new List<IDependency>();
        public IList<IDependency> Dependencies { get { return _dependencies; } }
        public string Name { get; set; }

        public DebugInfoMiddleware(
            IRequestRouter requestRouter,
            IHtmlWriterFactory htmlWriterFactory,
            IRenderContextFactory renderContextFactory)
        {
            _requestRouter = requestRouter;
            _htmlWriterFactory = htmlWriterFactory;
            _renderContextFactory = renderContextFactory;
            this.RunFirst();
        }

        public Task Invoke(IOwinContext context, Func<Task> next)
        {
            var debug = context.Request.Query["debug"];
            if (string.IsNullOrEmpty(debug)) 
                return next();

            var runable = _requestRouter.Route(context);
            if (runable == null)
            {
                context.Response.StatusCode = 404;
                return context.Response.WriteAsync("No routes match this request");
            }

            var debugInfo = runable.GetDebugInfo(0, -1);

            if (string.Equals("svg", debug))
            {
                var svg = new DebugSvgDrawing();
                return svg.Write(context, debugInfo);
            }

            if (string.Equals("html", debug))
            {
                var htmlPage = new DebugHtmlPage(_htmlWriterFactory, _renderContextFactory);
                return htmlPage.Write(context, debugInfo);
            }

            if (string.Equals("xml", debug))
                return WriteXml(context, debugInfo);

            if (string.Equals("json", debug))
                return WriteJson(context, debugInfo);

            var accept = context.Request.Accept;

            if (!string.IsNullOrEmpty(accept))
            {
                if (accept.Contains("image/svg+xml"))
                {
                    var svg = new DebugSvgDrawing();
                    return svg.Write(context, debugInfo);
                }

                if (accept.Contains("text/html"))
                {
                    var htmlPage = new DebugHtmlPage(_htmlWriterFactory, _renderContextFactory);
                    return htmlPage.Write(context, debugInfo);
                }

                if (accept.Contains("application/xml"))
                    return WriteXml(context, debugInfo);
            }

            return WriteJson(context, debugInfo);
        }

        private Task WriteJson(IOwinContext context, DebugInfo debugInfo)
        {
            context.Response.ContentType = "application/json";

            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };
            return context.Response.WriteAsync(JsonConvert.SerializeObject(debugInfo, settings));
        }

        private Task WriteXml(IOwinContext context, DebugInfo debugInfo)
        {
            var serializer = new XmlSerializer(
                typeof(DebugInfo), 
                new []
                {
                    typeof(DebugComponent),
                    typeof(DebugDataConsumer),
                    typeof(DebugDataProvider),
                    typeof(DebugDataProviderDependency),
                    typeof(DebugDataScope),
                    typeof(DebugDataScopeProvider),
                    typeof(DebugDataSupplier),
                    typeof(DebugDataSupply),
                    typeof(DebugDataScope),
                    typeof(DebugInfo),
                    typeof(DebugLayout),
                    typeof(DebugLayoutRegion),
                    typeof(DebugModule),
                    typeof(DebugPackage),
                    typeof(DebugPage),
                    typeof(DebugRegion),
                    typeof(DebugRenderContext),
                    typeof(DebugRoute),
                    typeof(DebugService),
                    typeof(DebugSuppliedDependency)
                });
            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream);
            serializer.Serialize(writer, debugInfo);

            context.Response.ContentType = "application/xml";
            return context.Response.WriteAsync(memoryStream.ToArray());
        }

    }
}
