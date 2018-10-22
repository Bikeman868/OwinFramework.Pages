using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            var documentTransformer = new DocumentTransformer(_stringBuilderFactory);
            var document = documentTransformer.ParseDocument("text/x-markdown", markdown);
            documentTransformer.CleanDocument(document, DocumentCleaning.MakeParagraphs | DocumentCleaning.RemoveBlankLines);

            Write(templateDefinition, document);

            return templateDefinition.Build();
        }

        private void Write(ITemplateDefinition template, IDocumentElement element)
        {
            BeginElement(template, element);

            if (element.Children != null)
                foreach (var child in element.Children)
                    Write(template, child);

            EndElement(template, element);
        }

        private bool BeginElement(ITemplateDefinition template, IDocumentElement element)
        {
            var breakElement = element as IBreakElement;
            var textElement = element as ITextElement;
            var nestedElement = element as INestedElement;
            var containerElement = element as IContainerElement;
            var styleElement = element as IStyleElement;
            var configurableElement = element as IConfigurableElement;
            var linkElement = element as ILinkElement;

            List<string> attributes = null;

            /*
            if (styleElement != null)
            {
                if (!string.IsNullOrEmpty(styleElement.ClassNames))
                {
                    _characterStream.State = HtmlStates.AttributeName;
                    _textWriter.Write(" class=");
                    _characterStream.State = HtmlStates.BeforeAttributeValue;
                    _textWriter.WriteQuotedString(styleElement.ClassNames);
                }
                if (styleElement.Styles != null && styleElement.Styles.Count > 0)
                {
                    var styles = styleElement.Styles.Aggregate(string.Empty, (s, kvp) => s + (s.Length > 0 ? " " : "") + kvp.Key + ":" + kvp.Value + ";");

                    _characterStream.State = HtmlStates.AttributeName;
                    _textWriter.Write(" style=");
                    _characterStream.State = HtmlStates.BeforeAttributeValue;
                    _textWriter.WriteQuotedString(styles);
                }
            }

            if (configurableElement != null)
            {
                if (configurableElement.Attributes != null)
                {
                    foreach (var attribute in configurableElement.Attributes)
                    {
                        _characterStream.State = HtmlStates.AttributeName;
                        _textWriter.Write(' ');
                        _textWriter.Write(attribute.Key);
                        _textWriter.Write("=");
                        _characterStream.State = HtmlStates.BeforeAttributeValue;
                        _textWriter.WriteQuotedString(attribute.Value);
                    }
                }
            }
            
             */

            var attributeParams = attributes == null ? null : attributes.ToArray();

            switch (element.ElementType)
            {
                case ElementTypes.Heading:
                    {
                        var headingLevel = 1;
                        if (nestedElement != null)
                            headingLevel = nestedElement.Level;
                        template.AddElementOpen("h" + headingLevel, attributeParams);
                    }
                    break;
            }

            if (textElement != null && !string.IsNullOrEmpty(textElement.Text))
            {
                template.AddText(null, textElement.Text);
            }

            return true;
        }

        private bool EndElement(ITemplateDefinition template, IDocumentElement element)
        {
            var breakElement = element as IBreakElement;
            var textElement = element as ITextElement;
            var nestedElement = element as INestedElement;
            var containerElement = element as IContainerElement;
            var styleElement = element as IStyleElement;
            var configurableElement = element as IConfigurableElement;
            var linkElement = element as ILinkElement;

            switch (element.ElementType)
            {
                case ElementTypes.Heading:
                    template.AddElementClose();
                    break;
            }

            return true;
        }
    }
}
