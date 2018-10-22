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
        protected void Write(ITemplateDefinition template, IDocumentElement document)
        {
            BeginElement(template, document);

            if (document.Children != null)
                foreach (var child in document.Children)
                    Write(template, child);

            EndElement(template, document);
        }

        private void BeginElement(ITemplateDefinition template, IDocumentElement element)
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
                    BeginBreakElement(template, breakElement, attributeParams);
                    break;

                case ElementTypes.Container:
                    BeginContainerElement(template, containerElement, attributeParams);
                    break;

                case ElementTypes.Heading:
                    BeginHeadingElement(template, nestedElement, attributeParams);
                    break;

                case ElementTypes.InlineText:
                    BeginInlineTextElement(template, styleElement, attributeParams);
                    break;

                case ElementTypes.Link:
                    BeginLinkElement(template, linkElement, attributes);
                    break;

                case ElementTypes.Paragraph:
                    BeginParagraphElement(template, element, attributeParams);
                    break;
            }

            if (textElement != null && !string.IsNullOrEmpty(textElement.Text))
            {
                template.AddText(null, textElement.Text);
            }
        }


        private void EndElement(ITemplateDefinition template, IDocumentElement element)
        {
            var linkElement = element as ILinkElement;

            switch (element.ElementType)
            {
                case ElementTypes.InlineText:
                case ElementTypes.Container:
                case ElementTypes.Heading:
                case ElementTypes.Paragraph:
                    template.AddElementClose();
                    break;
                case ElementTypes.Link:
                    if (linkElement != null && linkElement.LinkType == LinkTypes.Reference)
                        template.AddElementClose();
                    break;
            }
        }

        private static void BeginParagraphElement(ITemplateDefinition template, IDocumentElement element,
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
                    template.AddElementOpen("li", attributeParams);
                }
                else
                {
                    template.AddElementOpen("p", attributeParams);
                }
            }
            else
            {
                template.AddElementOpen("p", attributeParams);
            }
        }

        private static void BeginLinkElement(ITemplateDefinition template, ILinkElement linkElement, List<string> attributes)
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
                        template.AddElementOpen("a", attributes.ToArray());
                        break;
                    case LinkTypes.Image:
                        attributes.Add("src");
                        attributes.Add(linkElement.LinkAddress);
                        template.AddSelfClosingElement("img", attributes.ToArray());
                        break;
                    case LinkTypes.Iframe:
                        attributes.Add("src");
                        attributes.Add(linkElement.LinkAddress);
                        template.AddSelfClosingElement("iframe", attributes.ToArray());
                        break;
                }
            }
        }

        private static void BeginInlineTextElement(ITemplateDefinition template, IStyleElement styleElement,
            string[] attributeParams)
        {
            if (styleElement != null && styleElement.Styles != null && styleElement.Styles.Count == 1)
            {
                if (styleElement.Styles.ContainsKey("font-weight") && styleElement.Styles["font-weight"] == "bold")
                {
                    template.AddElementOpen("b");
                }
                else if (styleElement.Styles.ContainsKey("font-style") && styleElement.Styles["font-style"] == "italic")
                {
                    template.AddElementOpen("i");
                }
                else
                {
                    template.AddElementOpen("span", attributeParams);
                }
            }
            else
            {
                template.AddElementOpen("span", attributeParams);
            }
        }

        private static void BeginHeadingElement(ITemplateDefinition template, INestedElement nestedElement,
            string[] attributeParams)
        {
            {
                var headingLevel = 1;
                if (nestedElement != null)
                    headingLevel = nestedElement.Level;
                template.AddElementOpen("h" + headingLevel, attributeParams);
            }
        }

        private static void BeginContainerElement(ITemplateDefinition template, IContainerElement containerElement,
            string[] attributeParams)
        {
            if (containerElement != null)
            {
                switch (containerElement.ContainerType)
                {
                    case ContainerTypes.Division:
                        template.AddElementOpen("div", attributeParams);
                        break;
                    case ContainerTypes.BareList:
                        template.AddElementOpen("ul", "style", "list-style-type:none;");
                        break;
                    case ContainerTypes.NumberedList:
                        template.AddElementOpen("ol", attributeParams);
                        break;
                    case ContainerTypes.BulletList:
                        template.AddElementOpen("ul", attributeParams);
                        break;
                    case ContainerTypes.BlockQuote:
                        template.AddElementOpen("blockquote", attributeParams);
                        break;
                    case ContainerTypes.PreFormatted:
                        template.AddElementOpen("pre", attributeParams);
                        break;
                    case ContainerTypes.Table:
                        template.AddElementOpen("table", attributeParams);
                        break;
                    case ContainerTypes.TableHeader:
                        template.AddElementOpen("thead", attributeParams);
                        break;
                    case ContainerTypes.TableBody:
                        template.AddElementOpen("tbody", attributeParams);
                        break;
                    case ContainerTypes.TableFooter:
                        template.AddElementOpen("tfoot", attributeParams);
                        break;
                    case ContainerTypes.TableHeaderRow:
                    case ContainerTypes.TableFooterRow:
                    case ContainerTypes.TableDataRow:
                        template.AddElementOpen("tr", attributeParams);
                        break;
                    case ContainerTypes.TableDataCell:
                        template.AddElementOpen("td", attributeParams);
                        break;
                    default:
                        throw new Exception("Dpcument parser does not know hoe to write " +
                                            containerElement.ContainerType + " containers");
                }
            }
        }

        private static void BeginBreakElement(ITemplateDefinition template, IBreakElement breakElement, string[] attributeParams)
        {
            var breakType = BreakTypes.LineBreak;
            if (breakElement != null)
                breakType = breakElement.BreakType;

            switch (breakType)
            {
                case BreakTypes.LineBreak:
                    template.AddSelfClosingElement("br", attributeParams);
                    break;
                case BreakTypes.HorizontalRule:
                    template.AddSelfClosingElement("hr", attributeParams);
                    break;
            }
        }
    }
}
