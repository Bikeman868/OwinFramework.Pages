using System;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.DebugMiddleware
{
    /// <summary>
    /// This middleware extracts and returns debug information when ?debug=true is added to the URL
    /// </summary>
    public class DebugHtmlPage
    {
        private readonly IHtmlWriterFactory _htmlWriterFactory;
        private readonly IRenderContextFactory _renderContextFactory;

        public DebugHtmlPage(
            IHtmlWriterFactory htmlWriterFactory,
            IRenderContextFactory renderContextFactory)
        {
            _htmlWriterFactory = htmlWriterFactory;
            _renderContextFactory = renderContextFactory;
        }

        public Task Write(IOwinContext context, DebugInfo debugInfo)
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

            if (debugInfo.DataConsumer != null)
            {
                var consumerDescription = debugInfo.DataConsumer.ToString();
                var lines = consumerDescription.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length > 0)
                {
                    html.WriteElementLine("p", "Is a data consumer");
                    html.WriteOpenTag("ul");
                    foreach (var line in lines)
                        html.WriteElementLine("li", line.InitialCaps());
                    html.WriteCloseTag("ul");
                }
            }

            if (debugInfo.DependentComponents != null && debugInfo.DependentComponents.Count > 0)
            {
                html.WriteElementLine("p", "Dependent components");
                html.WriteOpenTag("ul");
                foreach (var component in debugInfo.DependentComponents)
                    html.WriteElementLine("li", component.GetDebugInfo().ToString().InitialCaps());
                html.WriteCloseTag("ul");
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
                    html.WriteElementLine("li", scope.ToString());
                html.WriteCloseTag("ul");
            }

            if (dataScopeProvider.DataSupplies != null && dataScopeProvider.DataSupplies.Count > 0)
            {
                html.WriteElementLine("p", "Data supplied in this scope");
                html.WriteOpenTag("ul");
                foreach (var dataSupplier in dataScopeProvider.DataSupplies)
                    html.WriteElementLine("li", dataSupplier.ToString().InitialCaps());
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
                        var renderContext = _renderContextFactory.Create((c, f) => { });
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

                    WriteHtml(html, debugRenderContext, depth - 1);
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
            if (renderContext != null && renderContext.Data != null)
            {
                foreach (var kv in renderContext.Data)
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

        private void WriteHtml(IHtmlWriter html, DebugRoute route, int depth)
        {
        }

        private void WriteHtml(IHtmlWriter html, DebugService service, int depth)
        {
        }

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
