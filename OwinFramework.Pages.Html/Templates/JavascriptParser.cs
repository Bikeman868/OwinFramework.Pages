using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Templates;

namespace OwinFramework.Pages.Html.Templates
{
    /// <summary>
    /// This is a template parser that inserts the contents
    /// of the template into the Javascript module asset of the
    /// element that renders it or directly into the page
    /// </summary>
    public class JavascriptParser: ITemplateParser
    {
        private readonly ITemplateBuilder _templateBuilder;

        public JavascriptParser(
            ITemplateBuilder templateBuilder)
        {
            _templateBuilder = templateBuilder;
            RenderIntoPage = true;
        }

        public bool RenderIntoPage { get; set; }

        public ITemplate Parse(TemplateResource[] resources, IPackage package)
        {
            var template = _templateBuilder.BuildUpTemplate().PartOf(package);
            foreach (var resource in resources) 
            {
                var encoding = resource.Encoding ?? Encoding.UTF8;
                var javascript = encoding.GetString(resource.Content);

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
            return template.Build();
        }
    }
}
