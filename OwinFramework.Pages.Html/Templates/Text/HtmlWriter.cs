using System;
using System.Linq;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Templates.Text
{
    internal class HtmlWriter
    {
        private readonly HtmlCharacterStream _characterStream;
        private readonly TextWriter _textWriter;
        private readonly bool _fragment;
        private readonly string _indent = "  ";

        private bool _preformatted;

        public HtmlWriter(System.IO.TextWriter writer, bool fragment)
        {
            _fragment = fragment;

            _characterStream = new HtmlCharacterStream(writer);
            _characterStream.State = HtmlStates.Data;

            _textWriter = new TextWriter(_characterStream);
            _textWriter.LinePrefix = string.Empty;
        }

        public bool WriteElementBegin(IDocumentElement element)
        {
            var breakElement = element as IBreakElement;
            var textElement = element as ITextElement;
            var nestedElement = element as INestedElement;
            var containerElement = element as IContainerElement;
            var styleElement = element as IStyleElement;
            var configurableElement = element as IConfigurableElement;
            var linkElement = element as ILinkElement;
            
            Action close = null;

            _characterStream.State = HtmlStates.TagOpen;
            switch (element.ElementType)
            {
                case ElementTypes.Document:
                    if (!_fragment)
                    {
                        _textWriter.EnsureNewLine();
                        _textWriter.Write("<!DOCTYPE html>");
                        _textWriter.WriteLineBreak();
                        _textWriter.Write("<html>");
                        Indent();
                        _textWriter.EnsureNewLine();
                        _textWriter.Write("<head><title></title></head>");
                        _textWriter.WriteLineBreak();
                        _textWriter.Write("<body>");
                        _textWriter.WriteLineBreak();
                        Indent();
                    }
                    break;

                case ElementTypes.Break:
                    var breakType = BreakTypes.LineBreak;
                    if (breakElement != null)
                        breakType = breakElement.BreakType;

                    _textWriter.EnsureNewLine();
                    switch (breakType)
                    {
                        case BreakTypes.LineBreak:
                            _textWriter.Write("<br");
                            break;
                        case BreakTypes.HorizontalRule:
                            _textWriter.Write("<hr");
                            break;
                    }
                    close = () =>
                        {
                            _characterStream.State = HtmlStates.SelfClosingStartTag;
                            _textWriter.Write(">");
                            _textWriter.WriteLineBreak();
                        };
                    break;

                case ElementTypes.Container:
                    if (containerElement != null)
                    {
                        _textWriter.EnsureNewLine();
                        close = () =>
                        {
                            _characterStream.State = HtmlStates.SelfClosingStartTag;
                            _textWriter.Write(">");
                        };
                        switch (containerElement.ContainerType)
                        {
                            case ContainerTypes.Division:
                                _textWriter.Write("<div");
                                break;
                            case ContainerTypes.BareList:
                                _textWriter.Write("<ul");
                                _characterStream.State = HtmlStates.AttributeName;
                                _textWriter.Write(" style=");
                                _characterStream.State = HtmlStates.BeforeAttributeValue;
                                _textWriter.WriteQuotedString("list-style-type:none;");
                                break;
                            case ContainerTypes.NumberedList:
                                _textWriter.Write("<ol");
                                break;
                            case ContainerTypes.BulletList:
                                _textWriter.Write("<ul");
                                break;
                            case ContainerTypes.BlockQuote:
                                _textWriter.Write("<blockquote");
                                break;
                            case ContainerTypes.PreFormatted:
                                _textWriter.Write("<pre");
                                close = () =>
                                {
                                    _characterStream.State = HtmlStates.SelfClosingStartTag;
                                    _textWriter.Write(">");
                                    _textWriter.EnsureNewLine();
                                };
                                _preformatted = true;
                                break;
                            case ContainerTypes.Table:
                                _textWriter.Write("<table");
                                break;
                            case ContainerTypes.TableHeader:
                                _textWriter.Write("<thead");
                                break;
                            case ContainerTypes.TableBody:
                                _textWriter.Write("<tbody");
                                break;
                            case ContainerTypes.TableFooter:
                                _textWriter.Write("<tfoot");
                                break;
                            case ContainerTypes.TableHeaderRow:
                            case ContainerTypes.TableFooterRow:
                            case ContainerTypes.TableDataRow:
                                _textWriter.Write("<tr");
                                break;
                            case ContainerTypes.TableDataCell:
                                _textWriter.Write("<td");
                                break;
                            default:
                                throw new Exception("HTML writer does not know hoe to write " + 
                                    containerElement.ContainerType + " containers");
                        }
                        Indent();
                    }
                    break;

                case ElementTypes.Heading:
                    var headingLevel = 1;
                    if (nestedElement != null)
                        headingLevel = nestedElement.Level;

                    _textWriter.EnsureNewLine();
                    _textWriter.Write("<h");
                    _textWriter.Write(headingLevel.ToString());
                    Indent();
                    close = () =>
                    {
                        _characterStream.State = HtmlStates.SelfClosingStartTag;
                        _textWriter.Write(">");
                    };
                    break;

                case ElementTypes.InlineText:
                    if (styleElement != null && styleElement.Styles != null && styleElement.Styles.Count == 1)
                    {
                        if (styleElement.Styles.ContainsKey("font-weight") && styleElement.Styles["font-weight"] == "bold")
                        {
                            _textWriter.Write("<b");
                            styleElement = null;
                        }
                        else if (styleElement.Styles.ContainsKey("font-style") && styleElement.Styles["font-style"] == "italic")
                        {
                            _textWriter.Write("<i");
                            styleElement = null;
                        }
                        else
                        {
                            _textWriter.Write("<span");
                        }
                    }
                    else
                    {
                        _textWriter.Write("<span");
                    }
                    close = () =>
                    {
                        _characterStream.State = HtmlStates.SelfClosingStartTag;
                        _textWriter.Write(">");
                    };
                    break;

                case ElementTypes.Link:
                    if (linkElement != null)
                    {
                        switch (linkElement.LinkType)
                        {
                            case LinkTypes.Reference:
                                _textWriter.Write("<a href=");
                                close = () =>
                                {
                                    _characterStream.State = HtmlStates.SelfClosingStartTag;
                                    _textWriter.Write(">");
                                    Indent();
                                };
                                break;
                            case LinkTypes.Image:
                                _textWriter.Write("<img src=");
                                close = () =>
                                {
                                    _characterStream.State = HtmlStates.SelfClosingStartTag;
                                    _textWriter.Write(">");
                                };
                                break;
                            case LinkTypes.Iframe:
                                _textWriter.Write("<iframe src=");
                                close = () =>
                                {
                                    _characterStream.State = HtmlStates.SelfClosingStartTag;
                                    _textWriter.Write(">");
                                };
                                break;
                        }
                        _characterStream.State = HtmlStates.BeforeAttributeValue;
                        _textWriter.WriteQuotedString(linkElement.LinkAddress);

                        if (!string.IsNullOrEmpty(linkElement.AltText))
                        {
                            _textWriter.Write(" alt=");
                            _textWriter.WriteQuotedString(linkElement.AltText);
                        }
                    }
                    break;

                case ElementTypes.Paragraph:
                    _textWriter.EnsureNewLine();
                    if (element.Parent.ElementType == ElementTypes.Container)
                    {
                        var parentContainer = element.Parent as IContainerElement;
                        if (parentContainer != null &&
                            (parentContainer.ContainerType == ContainerTypes.BareList ||
                             parentContainer.ContainerType == ContainerTypes.BulletList ||
                             parentContainer.ContainerType == ContainerTypes.NumberedList))
                        {
                            _textWriter.Write("<li");
                        }
                        else
                        {
                            _textWriter.Write("<p");
                        }
                    }
                    else
                    {
                        _textWriter.Write("<p");
                    }
                    Indent();
                    close = () =>
                    {
                        _characterStream.State = HtmlStates.SelfClosingStartTag;
                        _textWriter.Write(">");
                    };
                    break;

                case ElementTypes.RawText:
                    break;

                default:
                    return true;
            }

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

            if (close != null) close();

            if (textElement != null && !string.IsNullOrEmpty(textElement.Text))
            {
                _characterStream.State = HtmlStates.Data;
                if (_preformatted)
                    _characterStream.Write(textElement.Text);
                else
                    _textWriter.Write(textElement.Text);
            }

            return true;
        }

        public bool WriteElementEnd(IDocumentElement element)
        {
            var nestedElement = element as INestedElement;
            var containerElement = element as IContainerElement;
            var linkElement = element as ILinkElement;
            var styleElement = element as IStyleElement;

            _characterStream.State = HtmlStates.TagOpen;

            switch (element.ElementType)
            {
                case ElementTypes.Document:
                    if (!_fragment)
                    {
                        Outdent();
                        _textWriter.EnsureNewLine();
                        _textWriter.Write("</body>");
                        _textWriter.LinePrefix = string.Empty;
                        _textWriter.EnsureNewLine();
                        _textWriter.Write("</html>");
                    }
                    break;

                case ElementTypes.Container:
                    if (containerElement != null)
                    {
                        Outdent();
                        switch (containerElement.ContainerType)
                        {
                            case ContainerTypes.Division:
                                _textWriter.Write("</div>");
                                break;
                            case ContainerTypes.BareList:
                                _textWriter.Write("</ul>");
                                break;
                            case ContainerTypes.NumberedList:
                                _textWriter.Write("</ol>");
                                break;
                            case ContainerTypes.BulletList:
                                _textWriter.Write("</ul>");
                                break;
                            case ContainerTypes.BlockQuote:
                                _textWriter.Write("</blockquote>");
                                break;
                            case ContainerTypes.PreFormatted:
                                _preformatted = false;
                                _textWriter.Write("</pre>");
                                break;
                            case ContainerTypes.Table:
                                _textWriter.Write("</table>");
                                break;
                            case ContainerTypes.TableHeader:
                                _textWriter.Write("</thead>");
                                break;
                            case ContainerTypes.TableBody:
                                _textWriter.Write("</tbody>");
                                break;
                            case ContainerTypes.TableFooter:
                                _textWriter.Write("</tfoot>");
                                break;
                            case ContainerTypes.TableHeaderRow:
                            case ContainerTypes.TableFooterRow:
                            case ContainerTypes.TableDataRow:
                                _textWriter.Write("</tr>");
                                break;
                            case ContainerTypes.TableDataCell:
                                _textWriter.Write("</td>");
                                break;
                            default:
                                throw new Exception("HTML writer does not know how to close" +
                                    containerElement.ContainerType + " containers");
                        }
                        _textWriter.EnsureNewLine();
                    }
                    break;

                case ElementTypes.Heading:
                    var headingLevel = 1;
                    if (nestedElement != null)
                        headingLevel = nestedElement.Level;
                    
                    Outdent();
                    _textWriter.Write("</h");
                    _textWriter.Write(headingLevel.ToString());
                    _textWriter.Write(">");
                    _textWriter.EnsureNewLine();
                    break;

                case ElementTypes.InlineText:
                    if (styleElement != null && styleElement.Styles != null && styleElement.Styles.Count == 1)
                    {
                        if (styleElement.Styles.ContainsKey("font-weight") && styleElement.Styles["font-weight"] == "bold")
                        {
                            _textWriter.Write("</b>");
                        }
                        else if (styleElement.Styles.ContainsKey("font-style") && styleElement.Styles["font-style"] == "italic")
                        {
                            _textWriter.Write("</i>");
                        }
                        else
                        {
                            _textWriter.Write("</span>");
                        }
                    }
                    else
                    {
                        _textWriter.Write("</span>");
                    }
                    break;

                case ElementTypes.Link:
                    if (linkElement != null)
                    {
                        switch (linkElement.LinkType)
                        {
                            case LinkTypes.Reference:
                                Outdent();
                                _textWriter.Write("</a>");
                                break;
                        }
                    }
                    break;

                case ElementTypes.Paragraph:
                    Outdent();
                    if (element.Parent.ElementType == ElementTypes.Container)
                    {
                        var parentContainer = element.Parent as IContainerElement;
                        if (parentContainer != null &&
                            (parentContainer.ContainerType == ContainerTypes.BareList ||
                             parentContainer.ContainerType == ContainerTypes.BulletList ||
                             parentContainer.ContainerType == ContainerTypes.NumberedList))
                        {
                            _textWriter.Write("</li>");
                        }
                        else
                        {
                            _textWriter.Write("</p>");
                        }
                    }
                    else
                    {
                        _textWriter.Write("</p>");
                    }
                    _textWriter.EnsureNewLine();
                    break;
            }
            return true;
        }

        public void Write(IDocumentElement element)
        {
            WriteElementBegin(element);

            if (element.Children != null)
                foreach (var child in element.Children)
                    Write(child);

            WriteElementEnd(element);
        }

        private void Indent()
        {
            _textWriter.LinePrefix += _indent;
        }

        private void Outdent()
        {
            _textWriter.LinePrefix = _textWriter.LinePrefix.Substring(0, _textWriter.LinePrefix.Length - _indent.Length);
        }
    }
}
