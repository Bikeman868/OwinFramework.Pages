using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Owin;
using Newtonsoft.Json;
using OwinFramework.Builder;
using OwinFramework.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using Svg;

namespace OwinFramework.Pages.Core.Debug
{
    /// <summary>
    /// This middleware extracts and returns debug information when ?debug=true is added to the URL
    /// </summary>
    public class DebugInfoMiddleware: IMiddleware<object>
    {
        private readonly IRequestRouter _requestRouter;
        private readonly IHtmlWriterFactory _htmlWriterFactory;
        private readonly IList<IDependency> _dependencies = new List<IDependency>();
        public IList<IDependency> Dependencies { get { return _dependencies; } }
        public string Name { get; set; }

        public DebugInfoMiddleware(
            IRequestRouter requestRouter,
            IHtmlWriterFactory htmlWriterFactory)
        {
            _requestRouter = requestRouter;
            _htmlWriterFactory = htmlWriterFactory;
            this.RunFirst();
        }

        public Task Invoke(IOwinContext context, Func<Task> next)
        {
            if (context.Request.Query["debug"] != "true") return next();

            var runable = _requestRouter.Route(context);
            if (runable == null)
            {
                context.Response.StatusCode = 404;
                return context.Response.WriteAsync("No routes match this request");
            }

            var debugInfo = runable.GetDebugInfo();

            var accept = context.Request.Accept;

            if (!string.IsNullOrEmpty(accept))
            {
                if (accept.Contains("image/svg+xml"))
                    return WriteSvg(context, debugInfo);

                if (accept.Contains("text/html"))
                    return WriteHtml(context, debugInfo);

                if (accept.Contains("application/xml"))
                    return WriteXml(context, debugInfo);
            }

            return WriteJson(context, debugInfo);
        }

        #region JSON document

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

        #endregion

        #region XML document

        private Task WriteXml(IOwinContext context, DebugInfo debugInfo)
        {
            var serializer = new XmlSerializer(
                typeof(DebugInfo), 
                new []
                {
                    typeof(DebugComponent),
                    typeof(DebugDataProvider),
                    typeof(DebugElement),
                    typeof(DebugLayout),
                    typeof(DebugLayoutRegion),
                    typeof(DebugModule),
                    typeof(DebugPackage),
                    typeof(DebugPage),
                    typeof(DebugRegion),
                    typeof(DebugRoute),
                    typeof(DebugService)
                });
            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream);
            serializer.Serialize(writer, debugInfo);

            context.Response.ContentType = "application/xml";
            return context.Response.WriteAsync(memoryStream.ToArray());
        }

        #endregion

        #region Html document

        private Task WriteHtml(IOwinContext context, DebugInfo debugInfo)
        {
            using (var html = _htmlWriterFactory.Create())
            {
                html.WriteDocumentStart("en-US");
                
                html.WriteOpenTag("head");
                html.WriteElementLine("title", "Debug info for " + context.Request.Path);

                var styles = GetTextResource("html.css");
                if (styles != null)
                {
                    html.WriteOpenTag("style");
                    foreach (var style in styles.Replace("\r", "").Split('\n'))
                        html.WriteLine(style);
                    html.WriteCloseTag("style");
                }

                var script = GetTextResource("html.js");
                if (script != null)
                {
                    html.WriteScriptOpen();
                    foreach (var scriptLine in script.Replace("\r", "").Split('\n'))
                        html.WriteLine(scriptLine);
                    html.WriteScriptClose();
                }

                html.WriteCloseTag("head");
                html.WriteOpenTag("body");

                html.WriteElementLine("h1", "Debug information for " + context.Request.Path);
                WriteDebugInfo(html, debugInfo);

                html.WriteCloseTag("body");
                html.WriteDocumentEnd();

                context.Response.ContentType = "text/html";
                return html.ToResponseAsync(context);
            }
        }

        private void StartIndent(IHtmlWriter html)
        {
            html.WriteOpenTag("div");
            html.WriteElementLine("div", null, "class", "indent");
            html.WriteOpenTag("div", "class", "indented");
        }

        private void EndIndent(IHtmlWriter html)
        {
            html.WriteCloseTag("div");
            html.WriteCloseTag("div");
        }

        private void WriteDebugInfo(IHtmlWriter html, DebugInfo debugInfo)
        {
            html.WriteElementLine("h2", debugInfo.Type + " " + debugInfo.Name);
            if (debugInfo.Instance != null)
            {
                html.WriteOpenTag("p");
                html.WriteElementLine("i", debugInfo.Instance.GetType().FullName);
                html.WriteCloseTag("p");
            }

            if (debugInfo is DebugPage) WriteHtml(html, (DebugPage)debugInfo);
            if (debugInfo is DebugLayout) WriteHtml(html, (DebugLayout)debugInfo);
        }

        private void WriteHtml(IHtmlWriter html, DebugPage page)
        {
            if (page.Routes != null && page.Routes.Count > 0)
            {
                html.WriteElementLine("h3", "Page routes");
                foreach(var route in page.Routes)
                    html.WriteElementLine("p", "Route: " + route);
            }

            if (page.Layout != null)
            {
                StartIndent(html);
                WriteDebugInfo(html, page.Layout);
                EndIndent(html);
            }
        }

        private void WriteHtml(IHtmlWriter html, DebugLayout layout)
        {
            if (layout.ClonedFrom != null)
            {
                html.WriteElementLine("p", "Layout inherits from layout " + layout.ClonedFrom.Name);
            }
            
            if (layout.Regions != null && layout.Regions.Count > 0)
            {
                html.WriteElementLine("p", "Layout has regions " + string.Join(", ", layout.Regions.Select(r => r.Name)));
                foreach (var layoutRegion in layout.Regions)
                {
                    if (layoutRegion.Region == null)
                    {
                        html.WriteElementLine("p", "Region " + layoutRegion.Name + " inherits layout default content");
                        var inheritedRegions = layout.ClonedFrom == null ? null : layout.ClonedFrom.Regions;
                        if (inheritedRegions != null)
                        {
                            var inheritedRegion = inheritedRegions.FirstOrDefault(r => r.Name == layoutRegion.Name);
                            if (inheritedRegion != null)
                            {
                                StartIndent(html);
                                WriteDebugInfo(html, inheritedRegion.Region);
                                EndIndent(html);
                            }
                        }
                    }
                    else
                    {
                        html.WriteElementLine("p", "Region " + layoutRegion.Name + " contains");
                        StartIndent(html);
                        WriteDebugInfo(html, layoutRegion.Region);
                        EndIndent(html);
                    }
                }
            }

        }

        #endregion

        #region SVG drawing

        private Task WriteSvg(IOwinContext context, DebugInfo debugInfo)
        {
            var drawing = GetDrawing(debugInfo);

            string svg;
            using (var stream = new MemoryStream())
            {
                drawing.Write(stream);
                svg = Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);
            }

            return context.Response.WriteAsync(svg);
        }

        private SvgDocument GetDrawing(DebugInfo debugInfo)
        {
            var drawing = CreateSvg();
            Finalize(drawing);
            return drawing;
        }

        private SvgDocument CreateSvg()
        {
            var document = new SvgDocument
            {
                FontFamily = "Arial",
                FontSize = 12
            };

            var styles = GetTextResource("svg.css");
            if (!string.IsNullOrEmpty(styles))
            {
                var styleElement = new NonSvgElement("style") 
                {
                    Content = "\n" + styles
                };
                document.Children.Add(styleElement);
            }

            var script = GetTextResource("svg.js");
            if (!string.IsNullOrEmpty(script))
            {
                document.CustomAttributes.Add("onload", "init(evt)");
                var scriptElement = new NonSvgElement("script");
                scriptElement.CustomAttributes.Add("type", "text/ecmascript");
                scriptElement.Content = "\n" + script;
                document.Children.Add(scriptElement);
            }

            return document;
        }

        public void Finalize(SvgDocument document)
        {
            if (document != null)
            {
                var elements = new SvgElement[document.Children.Count];
                document.Children.CopyTo(elements, 0);

                document.Children.Clear();

                foreach (var element in elements.Where(e => !e.ContainsAttribute("visibility")))
                    document.Children.Add(element);

                foreach (var element in elements.Where(e => e.ContainsAttribute("visibility")))
                    document.Children.Add(element);
            }
        }

        public void SetSize(SvgDocument document, SvgUnit width, SvgUnit height)
        {
            document.Width = width;
            document.Height = height;
            document.ViewBox = new SvgViewBox(0, 0, width, height);
        }

        #endregion

        #region Embedded resources

        public string GetTextResource(string filename)
        {
            var scriptResourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames().FirstOrDefault(n => n.Contains(filename));
            if (scriptResourceName != null)
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(scriptResourceName))
                {
                    if (stream == null) return null;
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            return null;
        }

        #endregion
    }
}
