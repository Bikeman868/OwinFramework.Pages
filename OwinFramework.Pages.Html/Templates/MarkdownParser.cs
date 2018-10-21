using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Templates;

namespace OwinFramework.Pages.Html.Templates
{
    /// <summary>
    /// This is a template parser that converts Markdown format into
    /// Html. This is suitable for static content that is written in
    /// the Markdown file format.
    /// </summary>
    public class MarkdownParser: ITemplateParser
    {
        private readonly ITemplateBuilder _templateBuilder;

        public MarkdownParser(
            ITemplateBuilder templateBuilder)
        {
            _templateBuilder = templateBuilder;
        }

        public ITemplate Parse(byte[] template, Encoding encoding, IPackage package)
        {
            encoding = encoding ?? Encoding.UTF8;
            var markdown = encoding.GetString(template);
            var html = ParseMarkdown(markdown);

            return _templateBuilder.BuildUpTemplate()
                .PartOf(package)
                .AddHtml(html)
                .Build();
        }

        private string ParseMarkdown(string markdown)
        {
            // TODO: Parse markdown to Html
            return markdown;
        }
    }
}
