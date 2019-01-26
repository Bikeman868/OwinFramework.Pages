using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Templates;
using OwinFramework.Pages.Html.Templates.Text;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Templates
{
    /// <summary>
    /// This is a template parser that allows you to combine Html, JavaScript and
    /// CSS into a signle file. The Html will be written to the body of the page
    /// and the JavaScript and CSS will be deployed as assets.
    /// To identify the sections of the file start a line with two hyphens and the
    /// content type, for example "--css", "--javascript" or "--html"
    /// </summary>
    public class MultiPartParser: DocumentParser, ITemplateParser
    {
        private readonly IStringBuilderFactory _stringBuilderFactory;
        private readonly ITemplateBuilder _templateBuilder;

        /// <summary>
        /// Constructs a parser that can parse Markdown documents into templates
        /// </summary>
        public MultiPartParser(
            IStringBuilderFactory stringBuilderFactory,
            ITemplateBuilder templateBuilder)
        {
            _stringBuilderFactory = stringBuilderFactory;
            _templateBuilder = templateBuilder;
        }

        public ITemplate Parse(byte[] template, Encoding encoding, IPackage package)
        {
            encoding = encoding ?? Encoding.UTF8;
            var text = encoding.GetString(template);

            var templateDefinition = _templateBuilder.BuildUpTemplate();

            var section = "--html";

            var html = new List<string>();
            var javaScript = new List<string>();
            var css = new List<string>();

            var lines = text.Replace("\r", string.Empty).Split('\n').Select(s => s.Trim());
            foreach (var line in lines)
            {
                if (line.StartsWith("--"))
                {
                    section = line.ToLower().Trim();
                    continue;
                }

                if (section.StartsWith("--javascript"))
                {
                    javaScript.Add(line);
                }
                else if (section.StartsWith("--css"))
                {
                    css.Add(line);
                }
                else if (section.StartsWith("--html"))
                {
                    html.Add(line);
                }
            }

            if (html.Count > 0 || javaScript.Count > 0 || css.Count > 0)
            {
                templateDefinition.AddComponent(new AssetComponent(css, html, javaScript));
            }

            return templateDefinition.Build();
        }


        private class AssetComponent: IComponent
        {
            IModule IDeployable.Module { get; set; }
            AssetDeployment IDeployable.AssetDeployment { get; set; }
            IPackage IPackagable.Package { get; set; }
            ElementType INamed.ElementType { get { return ElementType.Component; } }
            string INamed.Name { get; set; }

            private object _lock = new object();
            private List<string> _css;
            private List<string> _html;
            private List<string> _javaScript;
            private PageArea[] _pageAreas;

            public AssetComponent(List<string> css, List<string> html, List<string> javaScript)
            {
                _css = css;
                _html = html;
                _javaScript = javaScript;
                _pageAreas = new PageArea[] { PageArea.Body };
            }

            IWriteResult IComponent.WritePageArea(IRenderContext context, PageArea pageArea)
            {
                if (pageArea == PageArea.Body)
                {
                    lock (_lock)
                    {
                        if (_css != null && _css.Count > 0)
                        {
                            context.Html.WriteOpenTag("style");
                            context.Html.WriteLine();

                            foreach (var line in _css)
                                if (!string.IsNullOrWhiteSpace(line))
                                    context.Html.WriteLine(line);

                            context.Html.WriteCloseTag("style");
                            context.Html.WriteLine();
                        }

                        if (_html != null && _html.Count > 0)
                        {
                            foreach (var line in _html)
                                if (!string.IsNullOrWhiteSpace(line))
                                    context.Html.WriteLine(line);
                        }

                        if (_javaScript != null && _javaScript.Count > 0)
                        {
                            context.Html.WriteScriptOpen();

                            foreach (var line in _javaScript)
                                if (!string.IsNullOrWhiteSpace(line))
                                    context.Html.WriteLine(line);

                            context.Html.WriteScriptClose();
                        }
                    }
                }
                return WriteResult.Continue();
            }

            IEnumerable<PageArea> IPageWriter.GetPageAreas()
            {
                return _pageAreas;
            }

            IWriteResult IPageWriter.WriteInPageStyles(ICssWriter writer, Func<ICssWriter, IWriteResult, IWriteResult> childrenWriter)
            {
                return WriteResult.Continue();
            }

            IWriteResult IPageWriter.WriteInPageFunctions(IJavascriptWriter writer, Func<IJavascriptWriter, IWriteResult, IWriteResult> childrenWriter)
            {
                return WriteResult.Continue();
            }

            IWriteResult IDeployable.WriteStaticCss(ICssWriter writer)
            {
                return WriteResult.Continue();
            }

            IWriteResult IDeployable.WriteStaticJavascript(IJavascriptWriter writer)
            {
                return WriteResult.Continue();
            }
        }
    }
}
