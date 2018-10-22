using System.Text;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Templates;
using OwinFramework.Pages.Html.Templates.Text;

namespace OwinFramework.Pages.Html.Templates
{
    /// <summary>
    /// This is a template parser that converts Markdown format into
    /// Html. This is suitable for static content that is written in
    /// the Markdown file format.
    /// </summary>
    public class MarkdownParser: DocumentParser, ITemplateParser
    {
        private readonly IStringBuilderFactory _stringBuilderFactory;
        private readonly ITemplateBuilder _templateBuilder;

        /// <summary>
        /// Constructs a parser that can parse Markdown documents into templates
        /// </summary>
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

            var documentTransformer = new DocumentTransformer(_stringBuilderFactory);
            var document = documentTransformer.ParseDocument("text/x-markdown", markdown);
            documentTransformer.CleanDocument(document, DocumentCleaning.MakeParagraphs | DocumentCleaning.RemoveBlankLines);

            var templateDefinition = _templateBuilder.BuildUpTemplate().PartOf(package);
            Write(templateDefinition, document);
            return templateDefinition.Build();
        }
    }
}
