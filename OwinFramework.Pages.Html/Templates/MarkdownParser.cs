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

        public ITemplate Parse(TemplateResource[] resources, IPackage package)
        {
            var template = _templateBuilder.BuildUpTemplate().PartOf(package);

            foreach (var resource in resources)
            {
                var encoding = resource.Encoding ?? Encoding.UTF8;
                var markdown = encoding.GetString(resource.Content);

                var documentTransformer = new DocumentTransformer(_stringBuilderFactory);
                var document = documentTransformer.ParseDocument("text/x-markdown", markdown);
                documentTransformer.CleanDocument(document, DocumentCleaning.MakeParagraphs | DocumentCleaning.RemoveBlankLines);

                Write(template, document);
            }
            return template.Build();
        }
    }
}
