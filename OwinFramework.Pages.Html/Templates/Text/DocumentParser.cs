using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Templates;

namespace OwinFramework.Pages.Html.Templates.Text
{
    /// <summary>
    /// This is a template parser that converts Markdown format into
    /// Html. This is suitable for static content that is written in
    /// the Markdown file format.
    /// </summary>
    public class DocumentParser
    {
        private class ParserContext
        {
            public bool IsPreFormatted;
            public ITemplateDefinition Template;
        }

        protected void Write(ITemplateDefinition template, IDocument document)
        {
            Write(new ParserContext { Template = template }, document);
        }

        private void Write(ParserContext context, IDocumentElement element)
        {
            BeginElement(context, element);

            if (element.Children != null)
                foreach (var child in element.Children)
                    Write(context, child);

            EndElement(context, element);
        }

        private void BeginElement(ParserContext context, IDocumentElement element)
        {
            var breakElement = element as IBreakElement;
            var textElement = element as ITextElement;
            var nestedElement = element as INestedElement;
            var containerElement = element as IContainerElement;
            var styleElement = element as IStyleElement;
            var configurableElement = element as IConfigurableElement;
            var linkElement = element as ILinkElement;

            List<string> attributes = null;

            if (styleElement != null)
            {
                if (!string.IsNullOrEmpty(styleElement.ClassNames))
                {
                    if (attributes == null) attributes = new List<string>();
                    attributes.Add("class");
                    attributes.Add(styleElement.ClassNames);
                }
                if (styleElement.Styles != null && styleElement.Styles.Count > 0)
                {
                    var styles = styleElement.Styles.Aggregate(string.Empty, (s, kvp) => s + (s.Length > 0 ? " " : "") + kvp.Key + ":" + kvp.Value + ";");

                    if (attributes == null) attributes = new List<string>();
                    attributes.Add("style");
                    attributes.Add(styles);
                }
            }

            if (configurableElement != null)
            {
                if (configurableElement.Attributes != null)
                {
                    if (attributes == null) attributes = new List<string>();
                    foreach (var attribute in configurableElement.Attributes)
                    {
                        attributes.Add(attribute.Key);
                        attributes.Add(attribute.Value);
                    }
                }
            }
            
            var attributeParams = attributes == null ? null : attributes.ToArray();

            switch (element.ElementType)
            {
                case ElementTypes.Break:
                    BeginBreakElement(context, breakElement, attributeParams);
                    break;

                case ElementTypes.Container:
                    BeginContainerElement(context, containerElement, attributeParams);
                    break;

                case ElementTypes.Heading:
                    BeginHeadingElement(context, nestedElement, attributeParams);
                    break;

                case ElementTypes.InlineText:
                    BeginInlineTextElement(context, styleElement, attributeParams);
                    break;

                case ElementTypes.Link:
                    BeginLinkElement(context, linkElement, attributes);
                    break;

                case ElementTypes.Paragraph:
                    BeginParagraphElement(context, element, attributeParams);
                    break;
            }

            if (textElement != null && !string.IsNullOrEmpty(textElement.Text))
            {
                context.Template.AddText(null, textElement.Text, context.IsPreFormatted);
            }
        }


        private void EndElement(ParserContext context, IDocumentElement element)
        {
            var linkElement = element as ILinkElement;

            switch (element.ElementType)
            {
                case ElementTypes.Container:
                    context.IsPreFormatted = false;
                    context.Template.AddElementClose();
                    context.Template.AddLineBreak();
                    break;

                case ElementTypes.InlineText:
                    context.Template.AddElementClose();
                    break;

                case ElementTypes.Heading:
                case ElementTypes.Paragraph:
                    context.Template.AddElementClose();
                    context.Template.AddLineBreak();
                    break;

                case ElementTypes.Link:
                    if (linkElement != null && linkElement.LinkType == LinkTypes.Reference)
                        context.Template.AddElementClose();
                    break;
            }
        }

        private static void BeginParagraphElement(ParserContext context, IDocumentElement element,
            string[] attributeParams)
        {
            if (element.Parent.ElementType == ElementTypes.Container)
            {
                var parentContainer = element.Parent as IContainerElement;
                if (parentContainer != null &&
                    (parentContainer.ContainerType == ContainerTypes.BareList ||
                     parentContainer.ContainerType == ContainerTypes.BulletList ||
                     parentContainer.ContainerType == ContainerTypes.NumberedList))
                {
                    context.Template.AddElementOpen("li", attributeParams);
                }
                else
                {
                    context.Template.AddElementOpen("p", attributeParams);
                }
            }
            else
            {
                context.Template.AddElementOpen("p", attributeParams);
            }
        }

        private static void BeginLinkElement(ParserContext context, ILinkElement linkElement, List<string> attributes)
        {
            if (linkElement != null)
            {
                if (attributes == null) attributes = new List<string>();
                if (!string.IsNullOrEmpty(linkElement.AltText))
                {
                    attributes.Add("alt");
                    attributes.Add(linkElement.AltText);
                }
                switch (linkElement.LinkType)
                {
                    case LinkTypes.Reference:
                        attributes.Add("href");
                        attributes.Add(linkElement.LinkAddress);
                        context.Template.AddElementOpen("a", attributes.ToArray());
                        break;
                    case LinkTypes.Image:
                        attributes.Add("src");
                        attributes.Add(linkElement.LinkAddress);
                        context.Template.AddSelfClosingElement("img", attributes.ToArray());
                        break;
                    case LinkTypes.Iframe:
                        attributes.Add("src");
                        attributes.Add(linkElement.LinkAddress);
                        context.Template.AddSelfClosingElement("iframe", attributes.ToArray());
                        break;
                }
            }
        }

        private static void BeginInlineTextElement(ParserContext context, IStyleElement styleElement,
            string[] attributeParams)
        {
            if (styleElement != null && styleElement.Styles != null && styleElement.Styles.Count == 1)
            {
                if (styleElement.Styles.ContainsKey("font-weight") && styleElement.Styles["font-weight"] == "bold")
                {
                    context.Template.AddElementOpen("b");
                }
                else if (styleElement.Styles.ContainsKey("font-style") && styleElement.Styles["font-style"] == "italic")
                {
                    context.Template.AddElementOpen("i");
                }
                else
                {
                    context.Template.AddElementOpen("span", attributeParams);
                }
            }
            else
            {
                context.Template.AddElementOpen("span", attributeParams);
            }
        }

        private static void BeginHeadingElement(ParserContext context, INestedElement nestedElement,
            string[] attributeParams)
        {
            {
                var headingLevel = 1;
                if (nestedElement != null)
                    headingLevel = nestedElement.Level;
                context.Template.AddElementOpen("h" + headingLevel, attributeParams);
            }
        }

        private static void BeginContainerElement(ParserContext context, IContainerElement containerElement,
            string[] attributeParams)
        {
            if (containerElement != null)
            {
                switch (containerElement.ContainerType)
                {
                    case ContainerTypes.Division:
                        context.Template.AddElementOpen("div", attributeParams);
                        break;
                    case ContainerTypes.BareList:
                        context.Template.AddElementOpen("ul", "style", "list-style-type:none;");
                        break;
                    case ContainerTypes.NumberedList:
                        context.Template.AddElementOpen("ol", attributeParams);
                        break;
                    case ContainerTypes.BulletList:
                        context.Template.AddElementOpen("ul", attributeParams);
                        break;
                    case ContainerTypes.BlockQuote:
                        context.Template.AddElementOpen("blockquote", attributeParams);
                        break;
                    case ContainerTypes.PreFormatted:
                        context.Template.AddElementOpen("pre", attributeParams);
                        context.IsPreFormatted = true;
                        break;
                    case ContainerTypes.Table:
                        context.Template.AddElementOpen("table", attributeParams);
                        break;
                    case ContainerTypes.TableHeader:
                        context.Template.AddElementOpen("thead", attributeParams);
                        break;
                    case ContainerTypes.TableBody:
                        context.Template.AddElementOpen("tbody", attributeParams);
                        break;
                    case ContainerTypes.TableFooter:
                        context.Template.AddElementOpen("tfoot", attributeParams);
                        break;
                    case ContainerTypes.TableHeaderRow:
                    case ContainerTypes.TableFooterRow:
                    case ContainerTypes.TableDataRow:
                        context.Template.AddElementOpen("tr", attributeParams);
                        break;
                    case ContainerTypes.TableDataCell:
                        context.Template.AddElementOpen("td", attributeParams);
                        break;
                    default:
                        throw new Exception("Document parser does not know how to write " +
                                            containerElement.ContainerType + " containers");
                }
            }
        }

        private static void BeginBreakElement(ParserContext context, IBreakElement breakElement, string[] attributeParams)
        {
            var breakType = BreakTypes.LineBreak;
            if (breakElement != null)
                breakType = breakElement.BreakType;

            switch (breakType)
            {
                case BreakTypes.LineBreak:
                    context.Template.AddSelfClosingElement("br", attributeParams);
                    break;
                case BreakTypes.HorizontalRule:
                    context.Template.AddSelfClosingElement("hr", attributeParams);
                    break;
            }
        }
    }
}
