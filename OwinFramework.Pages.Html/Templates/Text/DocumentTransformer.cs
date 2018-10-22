using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OwinFramework.Pages.Core.Interfaces.Collections;

namespace OwinFramework.Pages.Html.Templates.Text
{
    public class DocumentTransformer : IDocumentTransformer
    {
        private readonly IStringBuilderFactory _stringBuilderFactory;

        public DocumentTransformer(
            IStringBuilderFactory stringBuilderFactory)
        {
            _stringBuilderFactory = stringBuilderFactory;
        }

        public void ParseDocument(
            string mimeType,
            string documentContent,
            Func<IDocumentElement, bool> onBeginProcessElement,
            Func<IDocumentElement, bool> onEndProcessElement)
        {
#if DEBUG
            var originalHandler = onEndProcessElement;
            onEndProcessElement = e =>
            {
                if (e is DocumentElement)
                    OutputDocumentTree(e);

                return originalHandler(e);
            };
#endif
            if (string.Equals(mimeType, "text/html", StringComparison.OrdinalIgnoreCase))
                ParseHtml(documentContent, onBeginProcessElement, onEndProcessElement);

            else if (string.Equals(mimeType, "text/x-markdown", StringComparison.OrdinalIgnoreCase))
                ParseMarkdown(documentContent, onBeginProcessElement, onEndProcessElement);

            else throw new NotImplementedException("The current implementation of ITextAnalyser can not parse " + mimeType + " documents.");
        }

        public void ParseDocument(string mimeType, string documentContent, IDocumentProcessor documentProcessor)
        {
            ParseDocument(mimeType, documentContent, documentProcessor.BeginProcessElement, documentProcessor.EndProcessElement);
        }

        public IDocument ParseDocument(string mimeType, string documentContent)
        {
            if (string.IsNullOrEmpty(mimeType))
                throw new ArgumentNullException("mimeType");

            IDocument root = null;
            ParseDocument(mimeType, documentContent, null,
                e =>
                {
                    if (e.ElementType == ElementTypes.Document) 
                        root = e as IDocument;
                    return true;
                });
            return root;
        }

        public void FormatDocument(string mimeType, System.IO.TextWriter writer, IDocumentElement rootElement, bool fragment)
        {
            if (string.Equals(mimeType, "text/html", StringComparison.OrdinalIgnoreCase))
                FormatHtml(writer, rootElement, fragment);

            else if (string.Equals(mimeType, "text/x-markdown", StringComparison.OrdinalIgnoreCase))
                FormatMarkdown(writer, rootElement, fragment);

            else if (string.Equals(mimeType, "text/plain", StringComparison.OrdinalIgnoreCase))
                FormatPlainText(writer, rootElement, fragment);

            else throw new NotImplementedException();
        }

#if DEBUG
        private void OutputDocumentTree(IDocumentElement element)
        {
            Action<IDocumentElement, int> outputElement = null;
            outputElement = (e, d) =>
            {
                if (e.ElementType == ElementTypes.Document)
                {
                    if (e.Children != null && e.Children.Count > 0)
                    {
                        foreach (var child in e.Children)
                            outputElement((Element)child, d);
                    }
                    return;
                }

                var indent = new string(' ', d);

                var text = e as ITextElement;
                var configurable = e as IConfigurableElement;
                var container = e as IContainerElement;
                var style = e as IStyleElement;

                if (e.ElementType == ElementTypes.RawText && text != null)
                {
                    System.Diagnostics.Trace.WriteLine(indent + "'" + text.Text + "'");
                    return;
                }

                var line = string.Format("{0}<{1}[{2}{3}]",
                    indent, e.Name, e.ElementType, e.SuppressOutput ? "*" : "");

                if (configurable != null && configurable.Attributes != null)
                {
                    foreach (var attribute in configurable.Attributes)
                    {
                        line += " " + attribute.Key;
                        if (!string.IsNullOrEmpty(attribute.Value))
                            line += "='" + attribute.Value + "'";
                    }
                }

                if (style != null)
                {
                    if (style.Styles != null && style.Styles.Count > 0)
                    {
                        foreach (var attribute in style.Styles)
                        {
                            line += " " + attribute.Key;
                            if (!string.IsNullOrEmpty(attribute.Value))
                                line += ":" + attribute.Value;
                        }
                    }
                    if (!string.IsNullOrEmpty(style.ClassNames))
                    {
                        line += " class='" + style.ClassNames + "'";
                    }
                }

                if (container != null)
                {
                    if (e.Children == null || e.Children.Count == 0)
                    {
                        line += "{" + container.ContainerType + "} />";
                        System.Diagnostics.Trace.WriteLine(line);
                    }
                    else
                    {
                        line += ">";
                        line += "{" + container.ContainerType + "}";
                        System.Diagnostics.Trace.WriteLine(line);
                        foreach (var child in e.Children)
                            outputElement((Element)child, d + 2);
                        line = string.Format("{0}</{1}>", indent, e.Name);
                        line += "{" + container.ContainerType + "}";
                        System.Diagnostics.Trace.WriteLine(line);
                    }
                }
                else
                {
                    if (e.Children == null || e.Children.Count == 0)
                    {
                        line += " />";
                        System.Diagnostics.Trace.WriteLine(line);
                    }
                    else
                    {
                        line += ">";
                        System.Diagnostics.Trace.WriteLine(line);
                        foreach (var child in e.Children)
                            outputElement((Element)child, d + 2);
                        line = string.Format("{0}</{1}>", indent, e.Name);
                        System.Diagnostics.Trace.WriteLine(line);
                    }
                }
            };

            if (element != null) outputElement(element, 0);
        }
#endif

        private void ParseHtml(
            string documentContent,
            Func<IDocumentElement, bool> onBeginProcessElement,
            Func<IDocumentElement, bool> onEndProcessElement)
        {
            var parser = new HtmlParser(_stringBuilderFactory);
            var reader = new StringReader(documentContent);
            parser.Parse(reader, onBeginProcessElement, onEndProcessElement);
        }

        private void ParseMarkdown(
            string documentContent,
            Func<IDocumentElement, bool> onBeginProcessElement,
            Func<IDocumentElement, bool> onEndProcessElement)
        {
            var parser = new MarkdownParser(_stringBuilderFactory);
            var reader = new StringReader(documentContent);
            parser.Parse(reader, onBeginProcessElement, onEndProcessElement);
        }

        private void FormatHtml(System.IO.TextWriter writer, IDocumentElement rootElement, bool fragment)
        {
            var htmlWriter = new HtmlWriter(writer, fragment);
            htmlWriter.Write(rootElement);
        }

        private void FormatMarkdown(System.IO.TextWriter writer, IDocumentElement rootElement, bool fragment)
        {
            var markdownWriter = new MarkdownWriter(writer);
            markdownWriter.Write(rootElement);
        }

        private void FormatPlainText(System.IO.TextWriter writer, IDocumentElement rootElement, bool fragment)
        {
            var plainTextWriter = new PlainTextWriter(writer);
            plainTextWriter.Write(rootElement);
        }

        public void AdjustHeadings(IDocumentElement element, int minimumHeadingLevel)
        {
            Action<IDocumentElement, List<INestedElement>> addHeadings = null;
            addHeadings = (e, hl) =>
            {
                var h = e as INestedElement;
                if (e.ElementType == ElementTypes.Heading && h != null)
                    hl.Add(h);

                if (e.Children != null)
                    foreach (var child in e.Children)
                        addHeadings(child, hl);
            };

            var headings = new List<INestedElement>();
            addHeadings(element, headings);

            if (headings.Count > 0)
            {
                var lowestHeading = headings.Min(h => h.Level);
                if (lowestHeading > 1)
                {
                    foreach (var heading in headings)
                    {
                        heading.Level = heading.Level - lowestHeading + minimumHeadingLevel;
                    }
                }
            }
        }

        public void CleanAttributes(IDocumentElement element, Func<IDocumentElement, string, string, bool> attributeFilter)
        {
            if (element == null || attributeFilter == null) return;

            if (element.Children != null)
                foreach (var child in element.Children)
                    CleanAttributes(child, attributeFilter);
        }

        public void CleanElements(IDocumentElement element, Func<IDocumentElement, bool> elementFilter)
        {
            if (element == null || elementFilter == null || element.Children == null)
                return;

            for (var i = element.Children.Count - 1; i >= 0; i--)
            {
                var child = element.Children[i];
                if (elementFilter(child))
                    CleanElements(child, elementFilter);
                else
                    element.Children.RemoveAt(i);
            }
        }

        public void CleanDocument(IDocumentElement element, DocumentCleaning actions)
        {
            if (element == null || element.Children == null || element.Children.Count == 0)
                return;

            if ((actions & (DocumentCleaning.RemoveBlankLines | DocumentCleaning.MakeParagraphs)) != 0)
            {
                RemoveEmptyTextElements(element);
                CollapseLineBreaks(element);
            }

            if (actions.HasFlag(DocumentCleaning.MakeParagraphs))
                MakeParagraphs(element);

            foreach (var child in element.Children)
                CleanDocument(child, actions);

            if (actions.HasFlag(DocumentCleaning.RemoveBlankLines))
                RemoveEmptyContainers(element);
        }

        private void RemoveEmptyContainers(IDocumentElement element)
        {
            for (var i = element.Children.Count - 1; i >= 0; i--)
            {
                var child = element.Children[i];
                if (child != null &&
                    (child.ElementType == ElementTypes.Container || child.ElementType == ElementTypes.Paragraph) &&
                    (child.Children == null || child.Children.Count == 0))
                {
                    var container = child as IContainerElement;
                    if (container != null && container.ContainerType == ContainerTypes.TableDataCell)
                        continue; // Empty cells in tables are valid, deleting them moves all the other columns to the left

                    element.Children.RemoveAt(i);
                }
            }
        }

        private void RemoveEmptyTextElements(IDocumentElement element)
        {
            for (var i = element.Children.Count - 1; i >= 0; i--)
            {
                var child = element.Children[i];
                if (child != null &&
                    (child.ElementType == ElementTypes.RawText ||
                     child.ElementType == ElementTypes.InlineText) &&
                    (child.Children == null || child.Children.Count == 0))
                {
                    var text = child as ITextElement;
                    if (text != null && !string.IsNullOrEmpty(text.Text))
                        continue;

                    element.Children.RemoveAt(i);
                }
            }
        }

        private void CollapseLineBreaks(IDocumentElement element)
        {
            var nextIsLineBreak = false;
            for (var i = element.Children.Count - 2; i >= 0; i--)
            {
                var current = element.Children[i] as IBreakElement;
                if (current != null &&
                    current.BreakType == BreakTypes.LineBreak)
                {
                    if (nextIsLineBreak)
                    {
                        element.Children.RemoveAt(i + 1);
                    }
                    else
                    {
                        var next = element.Children[i + 1] as IBreakElement;
                        if (next != null &&
                            next.BreakType == BreakTypes.LineBreak)
                        {
                            element.Children.RemoveAt(i + 1);
                        }
                        nextIsLineBreak = true;
                    }
                }
                else
                {
                    i--;
                    nextIsLineBreak = false;
                }
            }
        }

        private void MakeParagraphs(IDocumentElement element)
        {
            for (var i = element.Children.Count - 1; i >= 0; i--)
            {
                var lineBreak = element.Children[i] as IBreakElement;
                {
                    if (lineBreak != null && lineBreak.BreakType == BreakTypes.LineBreak)
                    {
                        var first = i;

                        while (first > 0)
                        {
                            var textElement = element.Children[first - 1];
                            var linkElement = textElement as ILinkElement;

                            if (textElement.ElementType == ElementTypes.RawText ||
                                textElement.ElementType == ElementTypes.InlineText ||
                                (textElement.ElementType == ElementTypes.Link &&
                                 linkElement != null &&
                                 (linkElement.LinkType == LinkTypes.Reference || linkElement.LinkType == LinkTypes.Image))
                                )
                            {
                                first--;
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (first == i)
                            continue;

                        var paragraph = new ParagraphElement
                        {
                            Parent = element,
                            Children = new List<IDocumentElement>()
                        };
                        element.Children[i] = paragraph;

                        for (var j = first; j < i; j++)
                        {
                            var childToMove = element.Children[first];
                            paragraph.Children.Add(childToMove);
                            childToMove.Parent = paragraph;
                            element.Children.RemoveAt(first);
                        }

                        i = first;
                    }
                }
            }
        }
    }
}