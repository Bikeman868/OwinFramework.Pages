using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dotless.Core.configuration;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Templates;

namespace OwinFramework.Pages.Html.Templates
{
    /// <summary>
    /// This is a template parser that creates multi-part templates
    /// for components that have javascript, css and html in different
    /// files.
    /// It will:
    /// - Compile .less into .css
    /// - Include css and less in the styles part of the page
    /// - Include javascript in the script part of the page
    /// - Allow javascript strings to contain html by enclosing the html in back ticks
    /// - Supports mustache syntax in html files
    /// </summary>
    public class ComponentParser: ITemplateParser
    {
        private readonly ITemplateBuilder _templateBuilder;
        private readonly MustacheTemplateBuilder _mustacheTemplateBuilder;

        public ComponentParser(
            ITemplateBuilder templateBuilder)
        {
            _templateBuilder = templateBuilder;
            _mustacheTemplateBuilder = new MustacheTemplateBuilder();
            RenderIntoPage = true;
        }

        public bool RenderIntoPage { get; set; }

        public ITemplate Parse(TemplateResource[] resources, IPackage package)
        {
            var template = _templateBuilder.BuildUpTemplate().PartOf(package);
            foreach (var resource in resources) 
            {
                var encoding = resource.Encoding ?? Encoding.UTF8;
                var content = encoding.GetString(resource.Content);

                switch (resource.ContentType?.ToLower())
                {
                    case "application/javascript":
                        AddJavascriptToTemplate(template, content);
                        break;

                    case "text/css":
                        AddCssToTemplate(template, content);
                        break;

                    case "text/less":
                        AddLessToTemplate(template, content);
                        break;

                    case "text/html":
                        _mustacheTemplateBuilder.AddToTemplate(template, content);
                        break;

                    default:
                        template.AddHtml(content);
                        break;
                }
            }
            return template.Build();
        }

        private void AddCssToTemplate(ITemplateDefinition template, string css)
        {
            if (RenderIntoPage)
            {
                foreach (var line in css.Split('\n'))
                    template.AddStyleLine(line);
            }
            else
            {
                template.AddStaticCss(css);
            }
        }

        private void AddLessToTemplate(ITemplateDefinition template, string less)
        {
            var dotlessConfig = new DotlessConfiguration { MinifyOutput = true };
            var css = dotless.Core.Less.Parse(less, dotlessConfig);
            AddCssToTemplate(template, css);
        }

        private void AddJavascriptToTemplate(ITemplateDefinition template, string javascript)
        {
            const char backTick = '`';
            const char quote = '"';
            const char escape = '\\';
            const char newLine = '\n';
            var quoteString = new string(quote, 1);
            var escapedQuoteString = new string(new[] { escape, quote });
            var newLineString = new string(newLine, 1);
            var escapedNewLineString = new string(new[] { escape, 'n', escape, newLine });

            if (javascript.IndexOf(backTick) >= 0)
            {
                var sb = new StringBuilder();
                var s = javascript.Split(backTick);
                for (var i = 0; i < s.Length; i += 2)
                {
                    sb.Append(s[i]);
                    if (i + 1 < s.Length)
                    {
                        sb.Append(quote);
                        sb.Append(s[i + 1]
                            .Replace(quoteString, escapedQuoteString)
                            .Replace(newLineString, escapedNewLineString));
                        sb.Append(quote);
                    }
                }
                javascript = sb.ToString();
            }

            if (RenderIntoPage)
            {
                foreach (var line in javascript.Split('\n'))
                    template.AddInitializationLine(line);
            }
            else
            {
                template.AddStaticJavascript(javascript);
            }
        }
    }
}
