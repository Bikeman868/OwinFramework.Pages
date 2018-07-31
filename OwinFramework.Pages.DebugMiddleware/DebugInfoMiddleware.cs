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
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using Svg;

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

            var debugInfo = runable.GetDebugInfo();

            if (string.Equals("svg", debug))
                return WriteSvg(context, debugInfo);

            if (string.Equals("html", debug))
                return WriteHtml(context, debugInfo);

            if (string.Equals("xml", debug))
                return WriteXml(context, debugInfo);

            if (string.Equals("json", debug))
                return WriteJson(context, debugInfo);

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
                WriteDebugInfo(html, debugInfo, -1);

                html.WriteScriptOpen();
                html.WriteLine("var collapsibles = document.getElementsByClassName('indent');");
                html.WriteLine("for (i = 0; i < collapsibles.length; i++) {");
                html.WriteLine("  setCollapsible(collapsibles[i]);");
                html.WriteLine("  collapsibles[i].addEventListener('click', function () {");
                html.WriteLine("    this.classList.toggle('active');");
                html.WriteLine("    setCollapsible(this);");
                html.WriteLine("  });");
                html.WriteLine("}");
                html.WriteScriptClose();

                html.WriteCloseTag("body");
                html.WriteDocumentEnd();

                context.Response.ContentType = "text/html";
                return html.ToResponseAsync(context);
            }
        }

        private void StartIndent(IHtmlWriter html, bool expanded)
        {
            html.WriteOpenTag("div", "class", "section");
            html.WriteElementLine("button", "+", "class", expanded ? "indent active": "indent");
            html.WriteOpenTag("span", "class", "indented");
        }

        private void EndIndent(IHtmlWriter html)
        {
            html.WriteCloseTag("span");
            html.WriteCloseTag("div");
        }

        private void WriteDebugInfo(IHtmlWriter html, DebugInfo debugInfo, int depth)
        {
            if (depth == 0) return;

            html.WriteElementLine("h2", debugInfo.Type + " '" + debugInfo.Name + "'");
            if (debugInfo.Instance != null)
            {
                html.WriteOpenTag("p");
                html.WriteElementLine("i", debugInfo.Instance.GetType().DisplayName());
                html.WriteCloseTag("p");
            }

            if (debugInfo.DataConsumer != null && debugInfo.DataConsumer.Count > 0)
            {
                html.WriteElementLine("p", "Dependent data");
                html.WriteOpenTag("ul");
                foreach (var consumer in debugInfo.DataConsumer)
                    html.WriteElementLine("li", consumer);
                html.WriteCloseTag("ul");
            }

            if (debugInfo.DependentComponents != null && debugInfo.DependentComponents.Count > 0)
            {
                foreach(var component in debugInfo.DependentComponents)
                    html.WriteElementLine("p", "Needs component " + component.Name);
            }

            if (debugInfo is DebugComponent) WriteHtml(html, (DebugComponent)debugInfo, depth);
            if (debugInfo is DebugDataContext) WriteHtml(html, (DebugDataContext)debugInfo, depth);
            if (debugInfo is DebugDataProvider) WriteHtml(html, (DebugDataProvider)debugInfo, depth);
            if (debugInfo is DebugDataScopeProvider) WriteHtml(html, (DebugDataScopeProvider)debugInfo, depth);
            if (debugInfo is DebugLayout) WriteHtml(html, (DebugLayout)debugInfo, depth);
            if (debugInfo is DebugModule) WriteHtml(html, (DebugModule)debugInfo, depth);
            if (debugInfo is DebugPackage) WriteHtml(html, (DebugPackage)debugInfo, depth);
            if (debugInfo is DebugPage) WriteHtml(html, (DebugPage)debugInfo, depth);
            if (debugInfo is DebugRegion) WriteHtml(html, (DebugRegion)debugInfo, depth);
            if (debugInfo is DebugRenderContext) WriteHtml(html, (DebugRenderContext)debugInfo, depth);
            if (debugInfo is DebugRoute) WriteHtml(html, (DebugRoute)debugInfo, depth);
            if (debugInfo is DebugService) WriteHtml(html, (DebugService)debugInfo, depth);
        }

        private void WriteHtml(IHtmlWriter html, DebugComponent component, int depth)
        {
        }

        private void WriteHtml(IHtmlWriter html, DebugDataContext dataContext, int depth)
        {
            if (dataContext.Properties != null)
            {
                foreach (var property in dataContext.Properties)
                    html.WriteElementLine("p", "Data: " + property.DisplayName());
            }
        }

        private void WriteHtml(IHtmlWriter html, DebugDataProvider dataProvider, int depth)
        {
        }

        private void WriteHtml(IHtmlWriter html, DebugDataScopeProvider dataScopeProvider, int depth)
        {
            if (depth == 1) return;

            if (dataScopeProvider.Scopes != null && dataScopeProvider.Scopes.Count > 0)
            {
                html.WriteElementLine("p", "Dependencies resolved in this scope");
                html.WriteOpenTag("ul");
                foreach (var scope in dataScopeProvider.Scopes)
                    html.WriteElementLine("li", scope);
                html.WriteCloseTag("ul");
            }

            if (dataScopeProvider.DataSupplies != null && dataScopeProvider.DataSupplies.Count > 0)
            {
                html.WriteElementLine("p", "Data supplied in this scope");
                html.WriteOpenTag("ul");
                foreach (var dataSupplier in dataScopeProvider.DataSupplies)
                    html.WriteElementLine("li", dataSupplier);
                html.WriteCloseTag("ul");
            }

            if (dataScopeProvider.Parent != null)
            {
                if (depth == 1)
                {
                    html.WriteElementLine("p", "Has a parent scope");
                }
                else
                {
                    html.WriteElementLine("p", "Parent scope");
                    StartIndent(html, true);
                    WriteDebugInfo(html, dataScopeProvider.Parent, depth - 1);
                    EndIndent(html);
                }
            }

            if (dataScopeProvider.Children != null && dataScopeProvider.Children.Count > 0)
            {
                if (depth == 1)
                {
                    html.WriteElementLine("p", "Has " + dataScopeProvider.Children.Count + " child scopes");
                }
                else
                {
                    html.WriteElementLine("p", "Child scopes");
                    StartIndent(html, false);
                    foreach (var child in dataScopeProvider.Children)
                        WriteDebugInfo(html, child, depth - 1);
                    EndIndent(html);
                }
            }
        }

        private void WriteHtml(IHtmlWriter html, DebugLayout layout, int depth)
        {
            if (layout.InstanceOf != null)
            {
                html.WriteElementLine("p", "Layout inherits from '" + layout.InstanceOf.Name + "' layout");
                if (depth != 1)
                {
                    StartIndent(html, false);
                    WriteDebugInfo(html, layout.InstanceOf, 2);
                    EndIndent(html);
                }
            }
            
            if (layout.Regions != null && layout.Regions.Count > 0)
            {
                html.WriteElementLine("p", "Layout has regions " + string.Join(", ", layout.Regions.Select(r => "'" + r.Name + "'")));
                foreach (var layoutRegion in layout.Regions)
                {
                    if (layoutRegion.Region == null)
                    {
                        html.WriteElementLine("p", "Region '" + layoutRegion.Name + "' has default region for the layout");
                        var inheritedRegions = layout.InstanceOf == null ? null : layout.InstanceOf.Regions;
                        if (inheritedRegions != null)
                        {
                            var inheritedRegion = inheritedRegions.FirstOrDefault(r => r.Name == layoutRegion.Name);
                            if (inheritedRegion != null && depth != 1)
                            {
                                StartIndent(html, true);
                                WriteDebugInfo(html, inheritedRegion.Region, depth - 1);
                                EndIndent(html);
                            }
                        }
                    }
                    else
                    {
                        if (depth == 1)
                        {
                            html.WriteElementLine("p", "Region '" + layoutRegion.Name + "' contains '" + layoutRegion.Region.Name + "'");
                        }
                        else
                        { 
                            html.WriteElementLine("p", "Region '" + layoutRegion.Name + "' contents");
                            StartIndent(html, true);
                            WriteDebugInfo(html, layoutRegion.Region, depth - 1);
                            EndIndent(html);
                        }
                    }
                }
            }

        }

        private void WriteHtml(IHtmlWriter html, DebugModule module, int depth)
        {
        }

        private void WriteHtml(IHtmlWriter html, DebugPackage package, int depth)
        {
        }

        private void WriteHtml(IHtmlWriter html, DebugPage page, int depth)
        {
            if (page.Routes != null && page.Routes.Count > 0)
            {
                html.WriteElementLine("h3", "Page routes");
                foreach(var route in page.Routes)
                    html.WriteElementLine("p", "Route: " + route);
            }

            if (page.Scope != null)
            {
                html.WriteElementLine("p", "Page has a data scope");
                StartIndent(html, true);
                WriteDebugInfo(html, page.Scope, depth - 1);

                var dataScopeProvider = page.Scope.Instance as IDataScopeProvider;
                if (dataScopeProvider != null)
                {
                    DebugRenderContext debugRenderContext;
                    try
                    {
                        var renderContext = _renderContextFactory.Create();
                        dataScopeProvider.SetupDataContext(renderContext);
                        debugRenderContext = renderContext.GetDebugInfo();
                    }
                    catch (Exception ex)
                    {
                        debugRenderContext = null;
                        html.WriteElementLine("p", "Exception thrown when constructing data context tree: " + ex.Message);
                        if (!string.IsNullOrEmpty(ex.StackTrace))
                        {
                            html.WriteOpenTag("pre");
                            html.WriteLine(ex.StackTrace);
                            html.WriteCloseTag("pre");
                        }
                    }

                    if (debugRenderContext != null && debugRenderContext.Data != null)
                    {
                        foreach (var kv in debugRenderContext.Data)
                        {
                            var scopeId = kv.Key;
                            var dataContext = kv.Value;
                            if (dataContext != null)
                            {
                                if (dataContext.Properties == null || dataContext.Properties.Count == 0)
                                {
                                    html.WriteElementLine("h3", "Only dynamic data in context for data scope #" + scopeId);
                                }
                                else
                                {
                                    html.WriteElementLine("h3", "Static data in context for data scope #" + scopeId);
                                    html.WriteOpenTag("ul");
                                    foreach (var property in dataContext.Properties)
                                        html.WriteElementLine("li", property.DisplayName());
                                    html.WriteCloseTag("ul");
                                }
                            }
                        }
                    }
                }

                EndIndent(html);
            }

            if (page.Layout != null)
            {
                html.WriteElementLine("p", "Page has a layout");
                if (depth != 1)
                {
                    StartIndent(html, true);
                    WriteDebugInfo(html, page.Layout, depth - 1);
                    EndIndent(html);
                }
            }
        }

        private void WriteHtml(IHtmlWriter html, DebugRegion region, int depth)
        {
            if (region.InstanceOf != null)
            {
                html.WriteElementLine("p", "Region inherits from '" + region.InstanceOf.Name + "' region");
                if (depth != 1)
                {
                    StartIndent(html, false);
                    WriteDebugInfo(html, region.InstanceOf, 2);
                    EndIndent(html);
                }
            }

            if (region.RepeatType != null)
            {
                html.WriteOpenTag("p");
                html.Write("Repeat region for each ");
                html.WriteElement("i", region.RepeatType.DisplayName());
                if (!string.IsNullOrEmpty(region.RepeatScope))
                    html.Write(" in '" + region.RepeatScope + "' scope");
                html.Write(" from ");
                html.WriteElement("i", region.ListType.DisplayName());
                if (!string.IsNullOrEmpty(region.ListScope))
                    html.Write(" in '" + region.ListScope + "' scope");
                html.WriteCloseTag("p");
            }

            if (region.Scope != null)
            {
                if (region.Scope.Scopes != null)
                {
                    html.WriteElementLine("p", "Region data scope");
                    StartIndent(html, false);
                    WriteDebugInfo(html, region.Scope, 3);
                    EndIndent(html);
                }
            }

            if (region.Content != null)
            {
                html.WriteElementLine("p", "Region has contents");
                if (depth != 1)
                {
                    StartIndent(html, true);
                    WriteDebugInfo(html, region.Content, depth - 1);
                    EndIndent(html);
                }
            }
        }

        private void WriteHtml(IHtmlWriter html, DebugRenderContext renderContext, int depth)
        {
        }

        private void WriteHtml(IHtmlWriter html, DebugRoute route, int depth)
        {
        }

        private void WriteHtml(IHtmlWriter html, DebugService service, int depth)
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

            context.Response.ContentType = "image/svg+xml";
            return context.Response.WriteAsync(svg);
        }

        private SvgDocument GetDrawing(DebugInfo debugInfo)
        {
            var drawing = CreateSvg();

            // TODO: Make an SVG drawing of the page

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
