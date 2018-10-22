using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Collections;
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
        private readonly IStringBuilderFactory _stringBuilderFactory;
        private readonly ITemplateBuilder _templateBuilder;

        public MarkdownParser(
            IStringBuilderFactory stringBuilderFactory,
            ITemplateBuilder templateBuilder)
        {
            _stringBuilderFactory = stringBuilderFactory;
            _templateBuilder = templateBuilder;
        }

        public ITemplate Parse(byte[] template, Encoding encoding, IPackage package)
        {
            encoding = encoding ?? Encoding.UTF8;
            var markdown = encoding.GetString(template);

            var templateDefinition = _templateBuilder.BuildUpTemplate()
                .PartOf(package);

            var parser = new Text.MarkdownParser(_stringBuilderFactory);
            using (var textReader = new StringReader(markdown))
            {
                parser.Parse(
                    textReader,
                    e => BeginElement(templateDefinition, e),
                    e => EndElement(templateDefinition, e));
            }

            return templateDefinition.Build();
        }

        private bool BeginElement(ITemplateDefinition template, Text.IDocumentElement element)
        {
            template.AddText(null, "Begin " + element.ElementType);
            return true;
        }

        private bool EndElement(ITemplateDefinition template, Text.IDocumentElement element)
        {
            template.AddText(null, "End " + element.ElementType);
            return true;
        }
    }
}
