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
                if (!string.IsNullOrEmpty(script))
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
            html.WriteOpenTag("div", "class", "indent");
            html.WriteElementLine("span", null, "class", "indent");
            html.WriteOpenTag("span", "class", "indented");
        }

        private void EndIndent(IHtmlWriter html)
        {
            html.WriteCloseTag("span");
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

            if (debugInfo is DebugComponent) WriteHtml(html, (DebugComponent)debugInfo);
            if (debugInfo is DebugDataProvider) WriteHtml(html, (DebugDataProvider)debugInfo);
            if (debugInfo is DebugDataScopeProvider) WriteHtml(html, (DebugDataScopeProvider)debugInfo);
            if (debugInfo is DebugLayout) WriteHtml(html, (DebugLayout)debugInfo);
            if (debugInfo is DebugModule) WriteHtml(html, (DebugModule)debugInfo);
            if (debugInfo is DebugPackage) WriteHtml(html, (DebugPackage)debugInfo);
            if (debugInfo is DebugPage) WriteHtml(html, (DebugPage)debugInfo);
            if (debugInfo is DebugRegion) WriteHtml(html, (DebugRegion)debugInfo);
            if (debugInfo is DebugRoute) WriteHtml(html, (DebugRoute)debugInfo);
            if (debugInfo is DebugService) WriteHtml(html, (DebugService)debugInfo);
        }

        private void WriteHtml(IHtmlWriter html, DebugComponent component)
        {
        }

        private void WriteHtml(IHtmlWriter html, DebugDataProvider dataProvider)
        {
        }

        private void WriteHtml(IHtmlWriter html, DebugDataScopeProvider dataScopeProvider)
        {
            html.WriteElementLine("p", "Scope with id " + dataScopeProvider.Id);

            if (dataScopeProvider.Parent != null)
            {
                html.WriteElementLine("p", "Parent scope");
                StartIndent(html);
                WriteDebugInfo(html, dataScopeProvider.Parent);
                EndIndent(html);
            }

            if (dataScopeProvider.Children != null && dataScopeProvider.Children.Count > 0)
            {
                html.WriteElementLine("p", "Child scopes");
                StartIndent(html);
                foreach (var child in dataScopeProvider.Children)
                    WriteDebugInfo(html, child);
                EndIndent(html);
            }

            if (dataScopeProvider.Scopes != null && dataScopeProvider.Scopes.Count > 0)
            {
                html.WriteElementLine("p", "Scopes introduced");
                html.WriteOpenTag("ul");
                foreach (var scope in dataScopeProvider.Scopes)
                    html.WriteElementLine("li", scope);
                html.WriteCloseTag("ul");
            }

            if (dataScopeProvider.Dependencies != null && dataScopeProvider.Dependencies.Count > 0)
            {
                html.WriteElementLine("p", "Dependencies resolved");
                html.WriteOpenTag("ul");
                foreach (var dependencies in dataScopeProvider.Dependencies)
                    html.WriteElementLine("li", dependencies);
                html.WriteCloseTag("ul");
            }

            if (dataScopeProvider.DataProviders != null && dataScopeProvider.DataProviders.Count > 0)
            {
                html.WriteElementLine("p", "Data providers");
                html.WriteOpenTag("ul");
                foreach (var provider in dataScopeProvider.DataProviders)
                    WriteDebugInfo(html, provider);
                html.WriteCloseTag("ul");
            }
        }

        private void WriteHtml(IHtmlWriter html, DebugLayout layout)
        {
            if (layout.ClonedFrom != null)
            {
                html.WriteElementLine("p", "Layout inherits from " + layout.ClonedFrom.Name + " layout");
            }
            
            if (layout.Regions != null && layout.Regions.Count > 0)
            {
                html.WriteElementLine("p", "Layout has regions " + string.Join(", ", layout.Regions.Select(r => r.Name)));
                foreach (var layoutRegion in layout.Regions)
                {
                    if (layoutRegion.Region == null)
                    {
                        html.WriteElementLine("p", "Region " + layoutRegion.Name + " has default region for the layout");
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
                        html.WriteElementLine("p", "Region " + layoutRegion.Name + " overriden with");
                        StartIndent(html);
                        WriteDebugInfo(html, layoutRegion.Region);
                        EndIndent(html);
                    }
                }
            }

        }

        private void WriteHtml(IHtmlWriter html, DebugModule module)
        {
        }

        private void WriteHtml(IHtmlWriter html, DebugPackage package)
        {
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

        private void WriteHtml(IHtmlWriter html, DebugRegion region)
        {
            if (region.ClonedFrom != null)
            {
                html.WriteElementLine("p", "Region inherits from " + region.ClonedFrom.Name + " region");
            }

            if (region.Scope != null && 
                region.Scope.Scopes != null && 
                region.Scope.Scopes.Count > 0)
            {
                StartIndent(html);
                WriteDebugInfo(html, region.Scope);
                EndIndent(html);
            }

            if (region.Content != null)
            {
                html.WriteElementLine("p", "Region contains");
                StartIndent(html);
                WriteDebugInfo(html, region.Content);
                EndIndent(html);
            }
        }

        private void WriteHtml(IHtmlWriter html, DebugRoute route)
        {
        }

        private void WriteHtml(IHtmlWriter html, DebugService service)
        {
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
