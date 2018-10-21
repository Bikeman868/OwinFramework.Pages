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
    /// of the template directly into the page without changing
    /// anything. This is good for snippets of static Html, embedding
    /// SVG directly into the page etc.
    /// </summary>
    public class AsIsParser: ITemplateParser
    {
        private readonly ITemplateBuilder _templateBuilder;

        public AsIsParser(
            ITemplateBuilder templateBuilder)
        {
            _templateBuilder = templateBuilder;
        }

        public ITemplate Parse(byte[] template, Encoding encoding, IPackage package)
        {
            encoding = encoding ?? Encoding.UTF8;
            var html = encoding.GetString(template);

            return _templateBuilder.BuildUpTemplate()
                .PartOf(package)
                .AddHtml(html)
                .Build();
        }
    }
}
