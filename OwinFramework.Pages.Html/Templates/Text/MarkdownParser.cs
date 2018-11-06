using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OwinFramework.Pages.Core.Interfaces.Collections;

namespace OwinFramework.Pages.Html.Templates.Text
{
    // See https://github.com/adam-p/markdown-here/wiki/Markdown-Cheatsheet

    // For JavaScript MD editor see https://simplemde.com/

    internal class MarkdownParser
    {
        private readonly IStringBuilderFactory _stringBuilderFactory;

        private MarkdownCharacterStream _characterStream;
        private TextParser _stringParser;

        private Func<IDocumentElement, bool> _onBeginProcessElement;
        private Func<IDocumentElement, bool> _onEndProcessElement;

        private Core.Collections.LinkedList<StackedElement> _elementStack;
        private Dictionary<string, string> _references;
        private List<AnchorElement> _anchorsToFixup;
        private DocumentElement _document;

        public MarkdownParser(
            IStringBuilderFactory stringBuilderFactory)
        {
            _stringBuilderFactory = stringBuilderFactory;
        }

        public void Parse(
            TextReader reader,
            Func<IDocumentElement, bool> onBeginProcessElement, 
            Func<IDocumentElement, bool> onEndProcessElement)
        {
            _onBeginProcessElement = onBeginProcessElement;
            _onEndProcessElement = onEndProcessElement;

            _characterStream = new MarkdownCharacterStream(reader){State = MarkdownStates.ParagraphBreak};
            _stringParser = new TextParser(_stringBuilderFactory, _characterStream);
            _elementStack = new Core.Collections.LinkedList<StackedElement>();
            _references = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _anchorsToFixup = new List<AnchorElement>();

            _document = new DocumentElement
            {
                MimeType = "application/x-markdown",
                ConformanceLevel = 1.0f
            };

            PushElement(_document);

            var line = _stringBuilderFactory.Create();
            while (!_characterStream.Eof)
            {
                line.Clear();
                _stringParser.TakeUntil(line, 1024, '\n', false);
                AddLine(line.ToString());
            }

            while (!(_elementStack.Last().Element is DocumentElement))
                PopElement();

            FixUpReferences();

            End(_document);
        }

        private bool Begin(Element element)
        {
            if (element == null || element.SuppressOutput || _onBeginProcessElement == null) return true;
            return _onBeginProcessElement(element);
        }

        private bool End(Element element)
        {
            if (element == null || element.SuppressOutput || _onEndProcessElement == null) return true;
            return _onEndProcessElement(element);
        }

        private void FixUpReferences()
        {
            foreach (var anchor in _anchorsToFixup)
            {
                string referenceUrl;
                if (_references.TryGetValue(anchor.LinkAddress, out referenceUrl))
                {
                    anchor.LinkAddress = referenceUrl;
                }
            }
        }
    
        private void AddLine(string line)
        {
            if (CheckPreFormatted(line)) return;
            if (CheckBlankLine(line)) return;
            if (CheckDoubleUnderline(line)) return;
            if (CheckSingleUnderline(line)) return;
            if (CheckHorizontalRule(line)) return;

            var trimmedLine = line.Trim();

            if (CheckTable(trimmedLine)) return;
            if (CheckListItem(line, trimmedLine)) return;
            if (CheckHeading(trimmedLine)) return;
            if (CheckBlockQuote(trimmedLine)) return;

            AddParagraph(trimmedLine);
        }

        private bool CheckBlankLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                EndSection();
                return true;
            }
            return false;
        }

        private bool CheckDoubleUnderline(string rawLline)
        {
            if (rawLline.Length > 1 &&
                rawLline.All(c => c == '=') &&
                _characterStream.State == MarkdownStates.Paragraph)
            {
                var currentElement = CurrentElement as ParagraphElement;
                if (currentElement != null)
                    CurrentElement = new HeadingElement { Name = rawLline, Level = 1, Children = currentElement.Children };
                EndSection();
                return true;
            }
            return false;
        }

        private bool CheckSingleUnderline(string rawLine)
        {
            if (rawLine.Length > 1 &&
                rawLine.All(c => c == '-') &&
                _characterStream.State == MarkdownStates.Paragraph)
            {
                var currentElement = CurrentElement as ParagraphElement;
                if (currentElement != null)
                    CurrentElement = new HeadingElement { Name = rawLine, Level = 2, Children = currentElement.Children };
                EndSection();
                return true;
            }
            return false;
        }

        private bool CheckHorizontalRule(string rawLine)
        {
            if (_characterStream.State == MarkdownStates.ParagraphBreak &&
                (rawLine.StartsWith("---") ||
                 rawLine.StartsWith("***") ||
                 rawLine.StartsWith("___")))
            {
                PushElement(new BreakElement { Name = rawLine, BreakType = BreakTypes.HorizontalRule });
                PopElement();
                return true;
            }
            return false;
        }

        private bool CheckListItem(string rawLline, string trimmedLine)
        {
            if (_characterStream.State != MarkdownStates.ParagraphBreak &&
                _characterStream.State != MarkdownStates.NumberedList &&
                _characterStream.State != MarkdownStates.UnorderedList)
                return false;

            var firstWhitespace = trimmedLine.IndexOf(' ');
            if (firstWhitespace < 0) return false;

            var firstToken = trimmedLine.Substring(0, firstWhitespace);
            var text = trimmedLine.Substring(firstWhitespace + 1);

            var unorderedList = firstToken == "*" || firstToken == "+" || firstToken == "-";
            var numberedList = false;

            if (!unorderedList)
            {
                int i;
                numberedList = int.TryParse(firstToken.Substring(0, firstToken.Length - 1), out i);
            }

            if (unorderedList || numberedList)
            {
                if (CurrentElement is ParagraphElement)
                    PopElement();

                _characterStream.State = unorderedList ? MarkdownStates.UnorderedList : MarkdownStates.NumberedList;

                var isSubList = rawLline[0] == ' ';
                var containerType = unorderedList ? ContainerTypes.BulletList : ContainerTypes.NumberedList;
                var containerName = isSubList ? "SubList" : "List";

                var container = CurrentElement as ContainerElement;
                if (container == null || container.ContainerType != containerType || container.Name != containerName)
                {
                    if (container == null || (isSubList && container.Name != containerName))
                    {
                        // Start new sub-list
                        PushElement(new ContainerElement { ContainerType = containerType, Name = containerName });
                    }
                    else if (isSubList)
                    {
                        // End current sub-list
                        PopElement();

                        // Start a new sub-list
                        PushElement(new ContainerElement { ContainerType = containerType, Name = containerName });
                    }
                    else
                    {
                        // Close sub-list and return to outer list
                        PopElement();
                    }
                }
            }
            else
            {
                if (_characterStream.State != MarkdownStates.UnorderedList && _characterStream.State != MarkdownStates.NumberedList)
                    return false;
                text = trimmedLine;
            }

            if (CurrentElement is ParagraphElement)
                text = ' ' + text;
            else
                PushElement(new ParagraphElement { Name = "p" });


            ParseText(text);

            return true;
        }

        private bool CheckHeading(string trimmedLine)
        {
            if (trimmedLine[0] == '#' &&
                _characterStream.State == MarkdownStates.ParagraphBreak)
            {
                var level = 1;
                while (level < 6 && trimmedLine.Length > level && trimmedLine[level] == '#') 
                    level++;

                var start = level;
                while (trimmedLine.Length > start && char.IsWhiteSpace(trimmedLine[start]))
                    start++;

                if (trimmedLine.Length > start)
                {
                    var heading = new HeadingElement { Name = new String('#', level), Level = level };
                    PushElement(heading);
                    ParseText(trimmedLine.Substring(start));
                    EndSection();
                }

                return true;
            }
            return false;
        }

        private bool CheckBlockQuote(string trimmedLine)
        {
            if (_characterStream.State == MarkdownStates.ParagraphBreak && trimmedLine.StartsWith("> "))
            {
                _characterStream.State = MarkdownStates.BlockQuote;
                PushElement(new ContainerElement { Name = ">", ContainerType = ContainerTypes.BlockQuote });
            }

            if (_characterStream.State == MarkdownStates.BlockQuote)
            {
                if (trimmedLine.StartsWith("> "))
                    trimmedLine = trimmedLine.Substring(2);
                AppendToParagraph(trimmedLine);
                return true;
            }

            return false;
        }

        private bool CheckPreFormatted(string rawLine)
        {
            if (rawLine.StartsWith("```"))
            {
                if (_characterStream.State == MarkdownStates.ParagraphBreak)
                {
                    _characterStream.State = MarkdownStates.SourceCode;
                    PushElement(new ContainerElement { Name="```", ContainerType = ContainerTypes.PreFormatted });
                }
                else if (_characterStream.State == MarkdownStates.SourceCode)
                {
                    PopElement(); // Pop the container off the stack
                    _characterStream.State = MarkdownStates.ParagraphBreak;
                }
                return true;
            }

            if (_characterStream.State == MarkdownStates.SourceCode)
            {
                PushElement(new RawTextElement { Text = rawLine + "\n" });
                PopElement();
                return true;
            }

            return false;
        }

        private bool CheckTable(string trimmedLine)
        {
            Func<string, List<string>> splitColumns = l =>
                {
                    var columns = l.Split('|').Select(c => c.Trim()).ToList();
                    if (trimmedLine.StartsWith("|")) columns.RemoveAt(0);
                    if (trimmedLine.EndsWith("|")) columns.RemoveAt(columns.Count - 1);
                    return columns;
                };

            if ((_characterStream.State == MarkdownStates.ParagraphBreak && trimmedLine.Contains('|')) ||
                ((_characterStream.State == MarkdownStates.Paragraph || _characterStream.State == MarkdownStates.Heading) && trimmedLine[0] == '|'))
            {
                PushElement(new ContainerElement 
                { 
                    Name = "table", 
                    ContainerType = ContainerTypes.Table ,
                    ChildLayout = new List<string>()
                });
                PushElement(new ContainerElement
                {
                    Name = "thead",
                    ContainerType = ContainerTypes.TableHeader,
                });
                _characterStream.State = MarkdownStates.TableHeadings;
            }

            if (_characterStream.State == MarkdownStates.TableHeadings)
            {
                if (trimmedLine.All(c => char.IsWhiteSpace(c) || "|-:".Contains(c)))
                {
                    var columnFormats = splitColumns(trimmedLine);

                    for (var i = 0; i < columnFormats.Count; i++)
                    {
                        var format = columnFormats[i];
                        columnFormats[i] = string.Empty;
                        if (format.Length > 1)
                        {
                            if (format[format.Length - 1] == ':')
                            {
                                columnFormats[i] = format[0] == ':' ? "center" : "right";
                            }
                            else if (format[0] == ':')
                            {
                                columnFormats[i] = "left";
                            }
                        }
                    }

                    var tableElement = FindPriorElement<ContainerElement>(c => c.ContainerType == ContainerTypes.Table);
                    if (tableElement != null)
                        tableElement.ChildLayout = columnFormats;

                    PopElement(); // Pop the TableHeader

                    PushElement(new ContainerElement
                    {
                        Name = "tbody",
                        ContainerType = ContainerTypes.TableBody,
                    });

                    _characterStream.State = MarkdownStates.TableRow;
                }
                else
                {
                    PushElement(new ContainerElement { Name = "tr", ContainerType = ContainerTypes.TableHeaderRow });
                    var columns = splitColumns(trimmedLine);
                    foreach (var column in columns)
                    {
                        PushElement(new ContainerElement { Name = "th", ContainerType = ContainerTypes.TableDataCell });
                        ParseText(column);
                        PopElement(); // Pop the th off the stack, adding it to the tr
                    }
                    PopElement(); // Pop the tr, adding it to the table
                }
                return true;
            }

            if (_characterStream.State == MarkdownStates.TableRow)
            {
                var tableElement = FindPriorElement<ContainerElement>(c => c.ContainerType == ContainerTypes.Table);
                var columnFormats = tableElement == null ? null : (List<string>)tableElement.ChildLayout;

                PushElement(new ContainerElement { Name = "tr", ContainerType = ContainerTypes.TableDataRow });
                var columns = splitColumns(trimmedLine);
                for (var i = 0; i < columns.Count; i++ )
                {
                    var td = new ContainerElement { Name = "td", ContainerType = ContainerTypes.TableDataCell };
                    if (columnFormats != null && columnFormats.Count > i && !string.IsNullOrEmpty(columnFormats[i]))
                    {
                        td.Attributes = new Dictionary<string, string>
                        {
                            {"align", columnFormats[i]}
                        };
                    }
                    PushElement(td);
                    ParseText(columns[i]);
                    PopElement(); // Pop the th off the stack, adding it to the tr
                }
                PopElement(); // Pop the tr, adding it to the table
                return true;
            }

            return false;
        }

        private T FindPriorElement<T>(Func<T, bool> predicate) where T: Element
        {
            var listElement = _elementStack.LastOrDefault(e =>
            {
                var t = e.Element as T;
                return t != null && predicate(t);
            });

            return listElement == null ? null : (T)listElement.Data.Element;
        }

        private void AddParagraph(string trimmedLine)
        {
            AppendToParagraph(trimmedLine);
            _characterStream.State = MarkdownStates.Paragraph;
        }

        private void AppendToParagraph(string trimmedLine)
        {
            var currentParagraph = CurrentElement as ParagraphElement;

            if (currentParagraph == null)
            {
                currentParagraph = new ParagraphElement { Name = "p" };
                PushElement(currentParagraph);
            }
            else
            {
                PushElement(new RawTextElement { Text = " " });
                PopElement();
            }

            ParseText(trimmedLine);
        }

        private void ParseText(string text)
        {
            var isBold = false;
            var isItalic = false;
            var isCode = false;

            using (var buffer = _stringBuilderFactory.Create())
            {
                Action<IStringBuilder> flush = b =>
                    { 
                        if (b.Length > 0)
                        {
                            PushElement(new RawTextElement { Text = b.ToString() });
                            PopElement();
                            b.Clear();
                        }
                    };

                for(var i = 0; i < text.Length; i++)
                {
                    var prior2 = i > 1 ? text[i - 2] : default(char);
                    var prior1 = i > 0 ? text[i - 1] : default(char);
                    var current = text[i];
                    var next = i < text.Length - 2 ? text[i + 1] : default(char);

                    if ((prior1 == '*' && current == '*') || (prior1 == '_' && current == '_'))
                    {
                        // Double asterix or double underline turns bold on/off
                        flush(buffer);
                        if (isBold)
                        {
                            PopElement();
                            isBold = false;
                        }
                        else
                        {
                            PushElement(new FormattedElement
                            {
                                Name = new String(current, 2),
                                ElementType = ElementTypes.InlineText,
                                Styles = new Dictionary<string, string> { { "font-weight", "bold" } }
                            });
                            isBold = true;
                        }
                    }
                    else if ((prior1 == '*' || prior1 == '_') && (prior1 != prior2))
                    {
                        // Single asterix or single underline turns italic on/off
                        flush(buffer);
                        if (isItalic)
                        {
                            PopElement();
                            isItalic = false;
                        }
                        else
                        {
                            PushElement(new FormattedElement
                            {
                                Name = new String(prior1, 1),
                                ElementType = ElementTypes.InlineText,
                                Styles = new Dictionary<string, string> { { "font-style", "italic" } }
                            });
                            isItalic = true;
                        }
                        buffer.Append(current);
                    }
                    else if (current == '*' || current == '_')
                    {
                        // When we see the first asterix or underscore, we don't know yet if this
                        // is going to be bold or italic unless this is the last character in the string
                    }
                    else if (current == '`')
                    {
                        // Backticks turn on/off code formatting
                        flush(buffer);
                        if (isCode)
                        {
                            PopElement();
                            isCode = false;
                        }
                        else
                        {
                            PushElement(new FormattedElement
                            {
                                Name = new String(current, 2),
                                ElementType = ElementTypes.InlineText,
                                ClassNames = "code"
                            });
                            isCode = true;
                        }
                    }
                    else if (current == '!')
                    {
                        // Can be an image link if it is followed by []
                        if (next != '[')
                            buffer.Append(current);
                    }
                    else if (current == '[')
                    {
                        // Open square bracket is the start of a hyperlink

                        if (text.Length < i + 3)
                        {
                            buffer.Append(current);
                            continue;
                        }

                        var firstCloseIndex = text.IndexOf(']', i + 1);
                        if (firstCloseIndex < 0)
                        {
                            buffer.Append(current);
                            continue;
                        }

                        AnchorElement anchor = null;
                        var title = text.Substring(i + 1, firstCloseIndex - i - 1);
                        var nextChar = text.Length > firstCloseIndex + 3 ? text[firstCloseIndex + 1] : default(char);
                        var isImageLink = prior1 == '!';

                        if (nextChar == '(')
                        {
                            // Url is inline with the link
                            var secondCloseIndex = text.IndexOf(')', firstCloseIndex + 2);
                            if (secondCloseIndex == -1) secondCloseIndex = text.Length;
                            anchor = new AnchorElement
                            {
                                Name = "()",
                                LinkAddress = text.Substring(firstCloseIndex + 2, secondCloseIndex - firstCloseIndex - 2)
                            };
                            i = secondCloseIndex;
                        }
                        else if (nextChar == '[')
                        {
                            // Url is provided elsewhere in the document as a reference
                            var secondCloseIndex = text.IndexOf(']', firstCloseIndex + 2);
                            if (secondCloseIndex == -1) secondCloseIndex = text.Length;
                            anchor = new AnchorElement
                            {
                                Name = "[]",
                                LinkAddress = text.Substring(firstCloseIndex + 2, secondCloseIndex - firstCloseIndex - 2)
                            };
                            _anchorsToFixup.Add(anchor);
                            i = secondCloseIndex;
                        }
                        else if (nextChar == ':')
                        {
                            var urlStartIndex = firstCloseIndex + 2;
                            while (char.IsWhiteSpace(text[urlStartIndex])) urlStartIndex++;

                            var urlEndIndex = urlStartIndex + 1;
                            while (urlEndIndex < text.Length && !char.IsWhiteSpace(text[urlEndIndex])) urlEndIndex++;
                            var url = text.Substring(urlStartIndex, urlEndIndex - urlStartIndex);
                            _references[title] = url;
                            i = urlEndIndex - 1;
                        }
                        else
                        {
                            // Self-referencing
                            anchor = new AnchorElement
                            {
                                Name = "[]",
                                LinkAddress = title
                            };
                            _anchorsToFixup.Add(anchor);
                            i = firstCloseIndex;
                        }

                        if (anchor != null)
                        {
                            flush(buffer);
                            if (isImageLink)
                            {
                                anchor.LinkType = LinkTypes.Image;
                                anchor.AltText = title;
                                PushElement(anchor);
                                PopElement(); // Pop the anchor to add it to its parent
                            }
                            else
                            {
                                PushElement(anchor);
                                PushElement(new RawTextElement { Text = title });
                                PopElement(); // Pop the raw text
                                PopElement(); // Pop the anchor
                            }
                        }
                    }
                    else if (current == '<')
                    {
                        // Angle brackets are the start of a hyperlink with no title
                        var closeIndex = text.IndexOf('>', i + 1);
                        if (closeIndex < 0)
                        {
                            buffer.Append(current);
                            continue;
                        }
                        
                        var url = text.Substring(i + 1, closeIndex - i - 1);

                        flush(buffer);
                        PushElement(new AnchorElement { Name = "<>", LinkAddress = url });
                        PushElement(new RawTextElement { Text = url });
                        PopElement(); // Pop the raw text
                        PopElement(); // Pop the anchor

                        i = closeIndex;
                    }
                    else
                    {
                        buffer.Append(current);
                    }
                }

                flush(buffer);

                if (isItalic) // If italic was opened but not closed then close it here
                    PopElement();

                if (isBold) // If bold was opened but not closed then close it here
                    PopElement();

            }
        }

        #region Element stack

        private Element CurrentElement
        {
            get
            {
                var s = _elementStack.LastOrDefault();
                return s == null ? null : s.Element;
            }
            set
            {
                _elementStack.PopLast();
                PushElement(value);
            }
        }

        private void PushElement(Element element)
        {
            _elementStack.Append(new StackedElement { Element = element });
        }

        private bool PopElement()
        {
            if (_elementStack.IsEmpty) return true;

            foreach (var stackedElement in _elementStack)
            {
                if (!stackedElement.Started)
                {
                    stackedElement.Started = true;
                    if (!Begin(stackedElement.Element))
                        return false;
                }
            }

            var leaf = _elementStack.PopLast();

            var parent = _elementStack.LastOrDefault();
            if (parent != null)
            {
                leaf.Element.Parent = parent.Element;

                if (parent.Element.Children == null)
                    parent.Element.Children = new List<IDocumentElement> { leaf.Element };
                else
                    parent.Element.Children.Add(leaf.Element);
            }

            return End(leaf.Element);
        }

        private void EndSection()
        {
            while (!(CurrentElement is DocumentElement))
                PopElement();

            _characterStream.State = MarkdownStates.ParagraphBreak;
        }

        private class StackedElement
        {
            public bool Started;
            public Element Element;
        }

        #endregion
    }
}