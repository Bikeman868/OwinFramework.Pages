using System;
using System.Collections.Generic;
using System.Linq;

namespace OwinFramework.Pages.Html.Templates.Text
{
    internal class MarkdownWriter
    {
        private readonly MarkdownCharacterStream _characterStream;
        private readonly TextWriter _textWriter;

        private readonly List<int> _listItemNumber = new List<int>();
        private int _listLevel = -1;
        private int _tableColumnIndex = 0;

        public MarkdownWriter(System.IO.TextWriter writer)
        {
            _characterStream = new MarkdownCharacterStream(writer) 
            {
                State = MarkdownStates.ParagraphBreak
            };

            _textWriter = new TextWriter(_characterStream);
        }

        public bool Write(IDocumentElement element)
        {
            switch (element.ElementType)
            {
                case ElementTypes.Document:
                    return WriteDocument(element);
                case ElementTypes.Break:
                    return WriteBreak(element);
                case ElementTypes.Container:
                    return WriteContainer(element);
                case ElementTypes.Heading:
                    return WriteHeading(element);
                case ElementTypes.InlineText:
                    return WriteInlineText(element);
                case ElementTypes.Link:
                    return WriteLink(element);
                case ElementTypes.Paragraph:
                    return WriteParagraph(element);
            }

            var textElement = element as ITextElement;
            if (textElement != null && !string.IsNullOrEmpty(textElement.Text))
                _textWriter.Write(textElement.Text);

            return true;
        }

        private bool WriteBreak(IDocumentElement element)
        {
            var breakElement = element as IBreakElement;

            var breakType = BreakTypes.LineBreak;
            if (breakElement != null)
                breakType = breakElement.BreakType;

            _textWriter.EnsureBlankLine();

            switch (breakType)
            {
                case BreakTypes.HorizontalRule:
                    _characterStream.State = MarkdownStates.HorizontalRule;
                    _textWriter.Write("---");
                    _textWriter.EnsureBlankLine();
                    break;
            }

            return true;
        }

        private bool WriteContainer(IDocumentElement element)
        {
            var containerElement = element as IContainerElement;
            var configurableElement = element as IConfigurableElement;

            if (containerElement == null) return false;

            Action close = null;

            switch (containerElement.ContainerType)
            { 
                case ContainerTypes.BlockQuote:
                    _textWriter.EnsureBlankLine();
                    _characterStream.State = MarkdownStates.BlockQuote;
                    close = () => _textWriter.EnsureBlankLine();
                    break;

                case ContainerTypes.BulletList:
                    _listLevel++;
                    if (_listLevel == 0)
                        _textWriter.EnsureBlankLine();

                    _characterStream.State = MarkdownStates.UnorderedList;

                    close = () =>
                    {
                        if (_listLevel == 0)
                            _textWriter.EnsureBlankLine();
                        _listLevel--;
                    };
                    break;

                case ContainerTypes.NumberedList:
                    _listLevel++;
                    while (_listItemNumber.Count <= _listLevel) 
                        _listItemNumber.Add(0);
                    _listItemNumber[_listLevel] = 1;

                    if (_listLevel == 0)
                        _textWriter.EnsureBlankLine();

                    _characterStream.State = MarkdownStates.NumberedList;

                    close = () =>
                        {
                            if (_listLevel == 0)
                                _textWriter.EnsureBlankLine();
                            _listLevel--;
                        };
                    break;

                case ContainerTypes.PreFormatted:
                    _textWriter.EnsureBlankLine();
                    _characterStream.State = MarkdownStates.SourceCode;
                    _textWriter.Write("```");
                    _textWriter.WriteLineBreak();
                    close = () =>
                        {
                            _textWriter.EnsureNewLine();
                            _textWriter.Write("```");
                            _textWriter.EnsureBlankLine();
                        };
                    break;

                case ContainerTypes.Table:
                    containerElement.ChildLayout = new TableLayout();
                    _textWriter.EnsureBlankLine();
                    _characterStream.State = MarkdownStates.TableHeadings;
                    close = () => _textWriter.EnsureBlankLine();
                    break;

                case ContainerTypes.TableHeader:
                    close = () =>
                    {
                        var table = FindPriorElement<ContainerElement>(element, c => c.ContainerType == ContainerTypes.Table);
                        if (table == null) return;

                        var tableLayout = table.ChildLayout as TableLayout;
                        if (tableLayout == null) return;

                        var tableBody = table.Children
                            .Where(e => e.ElementType == ElementTypes.Container)
                            .Cast<IContainerElement>()
                            .FirstOrDefault(c => c.ContainerType ==  ContainerTypes.TableBody);

                        if (tableBody != null)
                        {
                            var tableBodyChildren = ((IDocumentElement)tableBody).Children;
                            if (tableBodyChildren != null)
                            {
                                foreach (var tableRow in tableBodyChildren
                                    .Where(c => c.ElementType == ElementTypes.Container && c.Children != null)
                                    .Cast<IContainerElement>()
                                    .Where(c => c.ContainerType == ContainerTypes.TableDataRow)
                                    .Cast<IDocumentElement>())
                                {
                                    var columnIndex = 0;
                                    foreach (var dataCell in tableRow.Children
                                        .Where(c => c.ElementType == ElementTypes.Container && c.Children != null)
                                        .Cast<IContainerElement>()
                                        .Where(c => c.ContainerType == ContainerTypes.TableDataCell)
                                        .Cast<IConfigurableElement>())
                                    {
                                        string alignment = null;
                                        if (dataCell != null && dataCell.Attributes != null && dataCell.Attributes.ContainsKey("align"))
                                        {
                                            alignment = dataCell.Attributes["align"];
                                        }
                                        tableLayout.SetColumn(columnIndex++, alignment, null);
                                    }
                                }
                            }
                        }

                        _textWriter.EnsureNewLine();
                        _textWriter.Write('|');

                        foreach (var tableColumn in tableLayout.Columns)
                        {
                            var padding = new string('-', tableColumn.Width);

                            switch (tableColumn.Alignment)
                            {
                                case "left":
                                    _textWriter.Write(':');
                                    _textWriter.Write(padding);
                                    _textWriter.Write(' ');
                                    break;
                                case "right":
                                    _textWriter.Write(' ');
                                    _textWriter.Write(padding);
                                    _textWriter.Write(':');
                                    break;
                                case "center":
                                    _textWriter.Write(':');
                                    _textWriter.Write(padding);
                                    _textWriter.Write(':');
                                    break;
                                default:
                                    _textWriter.Write(' ');
                                    _textWriter.Write(padding);
                                    _textWriter.Write(' ');
                                    break;
                            }
                            _textWriter.Write('|');
                        }
                    };
                    break;

                case ContainerTypes.TableHeaderRow:
                case ContainerTypes.TableDataRow:
                case ContainerTypes.TableFooterRow:
                    _textWriter.EnsureNewLine();
                    _characterStream.State = MarkdownStates.TableRow;
                    _textWriter.Write("|");
                    _tableColumnIndex = 0;
                    break;

                case ContainerTypes.TableDataCell:
                    {
                        var align = string.Empty;
                        if (configurableElement != null && configurableElement.Attributes != null)
                        {
                            if (configurableElement.Attributes.ContainsKey("align"))
                                align = configurableElement.Attributes["align"];
                        }

                        var table = FindPriorElement<ContainerElement>(element, c => c.ContainerType == ContainerTypes.Table);
                        var tableLayout = (TableLayout)table.ChildLayout;
                        tableLayout.SetColumn(_tableColumnIndex, align, null);
                        _tableColumnIndex++;

                        _characterStream.State = MarkdownStates.TableRow;
                        _textWriter.Write(" ");
                        close = () => _textWriter.Write(" |");
                        break;
                    }
            }

            if (element.Children != null)
                foreach (var child in element.Children)
                    Write(child);

            if (close != null) close();

            return true;
        }

        private bool WriteDocument(IDocumentElement element)
        {
            if (element.Children != null)
                foreach (var child in element.Children)
                    Write(child);

            return true;
        }

        private bool WriteHeading(IDocumentElement element)
        {
            var nestedElement = element as INestedElement;

            var headingLevel = 1;
            if (nestedElement != null)
                headingLevel = nestedElement.Level;

            _characterStream.State = MarkdownStates.Heading;

            _textWriter.EnsureBlankLine();
            _textWriter.Write(new string('#', headingLevel));
            _textWriter.Write(' ');

            if (element.Children != null)
                foreach (var child in element.Children)
                    Write(child);

            _textWriter.EnsureNewLine();

            return true;
        }

        private bool WriteInlineText(IDocumentElement element)
        {
            var textElement = element as ITextElement;
            var styleElement = element as IStyleElement;

            Action close = null;

            if (styleElement != null && styleElement.Styles != null)
            {
                if (styleElement.Styles.ContainsKey("font-weight") && styleElement.Styles["font-weight"] == "bold")
                {
                    _textWriter.Write("**");
                    close = () => _textWriter.Write("**");
                }
                else if (styleElement.Styles.ContainsKey("font-style") && styleElement.Styles["font-style"] == "italic")
                {
                    _textWriter.Write("_");
                    close = () => _textWriter.Write("_");
                }
            }

            if (textElement != null)
                _textWriter.Write(textElement.Text);

            if (element.Children != null)
                foreach (var child in element.Children)
                    Write(child);

            if (close != null) close();

            return true;
        }

        private bool WriteLink(IDocumentElement element)
        {
            var linkElement = element as ILinkElement;
            var textElement = element as ITextElement;
            
            if (linkElement == null) return false;

            Action<IDocumentElement> writeChildText = null;
            writeChildText = (e) =>
                {
                    if (e.Children != null)
                    {
                        foreach (var child in e.Children)
                        {
                            var childText = child as ITextElement;
                            if (childText != null)
                                _textWriter.Write(childText.Text);
                            writeChildText(child);
                        }
                    }
                };

            switch (linkElement.LinkType)
            {
                case LinkTypes.Reference:
                    if (textElement != null && !string.IsNullOrEmpty(textElement.Text))
                    {
                        _textWriter.Write("[");
                        _textWriter.Write(textElement.Text);
                    }
                    else if (element.Children != null && element.Children.Count > 0)
                    {
                        _textWriter.Write("[");
                        writeChildText(element);
                    }
                    else
                    {
                        _textWriter.Write("<");
                        _textWriter.Write(linkElement.LinkAddress);
                        _textWriter.Write(">");
                        return true;
                    }
                    break;

                case LinkTypes.Image:
                    _textWriter.Write("![");

                    if (!string.IsNullOrEmpty(linkElement.AltText))
                        _textWriter.Write(linkElement.AltText);

                    else if (textElement != null && !string.IsNullOrEmpty(textElement.Text))
                        _textWriter.Write(textElement.Text);

                    break;
            }

            _textWriter.Write("](");
            _textWriter.Write(linkElement.LinkAddress);
            _textWriter.Write(")");

            return true;
        }

        private bool WriteParagraph(IDocumentElement element)
        {
            var textElement = element as ITextElement;
            var styleElement = element as IStyleElement;

            Action endParagraph = null;

            Action beginParagraph = () =>
            {
                if (_characterStream.State == MarkdownStates.Heading)
                    _textWriter.EnsureNewLine();
                else
                    _textWriter.EnsureBlankLine();
                _characterStream.State = MarkdownStates.Paragraph;

                endParagraph = () => _textWriter.EnsureBlankLine();
            };

            Action beginBlockQuote = () =>
            {
                _characterStream.State = MarkdownStates.BlockQuote;
                styleElement = null;
                endParagraph = () => _textWriter.Write('\n');
            };

            Action beginPreformatted = () =>
            {
                _characterStream.State = MarkdownStates.SourceCode;
                _textWriter.EnsureNewLine();
                styleElement = null;
            };

            Action beginBulletList = () =>
            {
                _textWriter.EnsureNewLine();
                _characterStream.State = MarkdownStates.UnorderedList;

                if (_listLevel > 0)
                    _textWriter.Write(new string(' ', _listLevel * 2));
                _textWriter.Write("* ");

                endParagraph = () => _textWriter.EnsureNewLine();
            };

            Action beginNumberedList = () =>
            {
                _textWriter.EnsureNewLine();
                _characterStream.State = MarkdownStates.NumberedList;

                if (_listLevel > 0)
                    _textWriter.Write(new string(' ', _listLevel * 2));

                _textWriter.Write(_listItemNumber[_listLevel].ToString());
                _textWriter.Write(". ");
                _listItemNumber[_listLevel]++;

                endParagraph = () => _textWriter.EnsureNewLine();
            };

            var parentContainer = element.Parent as IContainerElement;
            if (parentContainer == null)
            {
                beginParagraph();
            }
            else
            { 
                if (parentContainer.ContainerType == ContainerTypes.BareList ||
                    parentContainer.ContainerType == ContainerTypes.BulletList)
                {
                    beginBulletList();
                }
                else if (parentContainer.ContainerType == ContainerTypes.NumberedList)
                {
                    beginNumberedList();
                }
                else if (parentContainer.ContainerType == ContainerTypes.BlockQuote)
                {
                    beginBlockQuote();
                }
                else if (parentContainer.ContainerType == ContainerTypes.PreFormatted)
                {
                    beginPreformatted();
                }
                else
                {
                    beginParagraph();
                }
            }

            Action closeStyle = null;

            if (styleElement != null && styleElement.Styles != null)
            {
                if (styleElement.Styles.ContainsKey("font-weight") && styleElement.Styles["font-weight"] == "bold")
                {
                    _textWriter.Write("**");
                    closeStyle = () => _textWriter.Write("**");
                }
                else if (styleElement.Styles.ContainsKey("font-style") && styleElement.Styles["font-style"] == "italic")
                {
                    _textWriter.Write("_");
                    closeStyle = () => _textWriter.Write("_");
                }
            }

            if (textElement != null && !string.IsNullOrEmpty(textElement.Text))
                _textWriter.Write(textElement.Text);

            if (element.Children != null)
            {
                foreach (var child in element.Children)
                    Write(child);
            }

            if (closeStyle != null) closeStyle();
            if (endParagraph != null) endParagraph();

            return true;
        }

        private T FindPriorElement<T>(IDocumentElement start, Func<T, bool> predicate) where T : Element
        {
            while (start != null)
            {
                var t = start as T;
                if (t != null && predicate(t))
                    return t;
                start = start.Parent;
            }
            return null;
        }

        private class TableLayout
        {
            public List<TableColumn> Columns = new List<TableColumn>();

            public void SetColumn(int index, string alignment, int? width)
            {
                while (index >= Columns.Count)
                    Columns.Add(new TableColumn { Alignment = null, Width = 3 });

                if (!string.IsNullOrEmpty(alignment) && string.IsNullOrEmpty(Columns[index].Alignment))
                    Columns[index].Alignment = alignment;

                if (width.HasValue && width.Value > Columns[index].Width)
                    Columns[index].Width = width.Value;
            }
        }

        private class TableColumn
        {
            public int Width;
            public string Alignment;
        }
    }
}
