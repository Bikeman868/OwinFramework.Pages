using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OwinFramework.Pages.Core.Interfaces.Collections;

namespace OwinFramework.Pages.Html.Templates.Text
{
    //  This code is based on https://www.w3.org/TR/html/syntax.html#parsing-html-documents

    internal class HtmlParser
    {
        private readonly IStringBuilderFactory _stringBuilderFactory;

        private const int _maximumNameLength = 20;
        private const int _maximumAttributeValueLength = 2000;

        private HtmlCharacterStream _characterStream;
        private TextParser _stringParser;
        private Func<IDocumentElement, bool> _onBeginProcessElement;
        private Func<IDocumentElement, bool> _onEndProcessElement;
        private DocumentElement _document;

        public HtmlParser(
            IStringBuilderFactory stringBuilderFactory)
        {
            _stringBuilderFactory = stringBuilderFactory;

            _allElements = new List<string>();
            _allElements.AddRange(_voidElements);
            _allElements.AddRange(_rawElements);
            _allElements.AddRange(_escapableRawElements);
            _allElements.AddRange(_normalElements);
        }

        public void Parse(
            TextReader reader,
            Func<IDocumentElement, bool> onBeginProcessElement, 
            Func<IDocumentElement, bool> onEndProcessElement)
        {
            _onBeginProcessElement = onBeginProcessElement;
            _onEndProcessElement = onEndProcessElement;

            _characterStream = new HtmlCharacterStream(reader)
            {
                State = HtmlStates.Data
            };

            _stringParser = new TextParser(_stringBuilderFactory, _characterStream);

            _document = new DocumentElement 
            { 
                MimeType = "text/html", 
                ConformanceLevel = 1.0f 
            };

            if (!Begin(_document)) return;

            Element currentElement = _document;
            while (!_characterStream.Eof)
                currentElement = ProcessCurrentState(currentElement);

            if (_document.Children == null || _document.Children.Count == 0)
            {
                _document.ConformanceLevel = 0;
            }
            else
            {
                var visibleChildren = _document.Children.Where(e => !e.SuppressOutput).ToList();
                if (visibleChildren.Count == 0 || visibleChildren.All(e => e.ElementType == ElementTypes.RawText))
                    _document.ConformanceLevel = 0;
            }

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

        private Element ProcessCurrentState(Element currentElement)
        {
            switch (_characterStream.State)
            {
                case HtmlStates.Data:
                    return ProcessDataState(currentElement);

                case HtmlStates.TagOpen:
                    return ProcessTagOpenState(currentElement);

                case HtmlStates.SelfClosingStartTag:
                    return ProcessSelfClosingStartTagState(currentElement);

                case HtmlStates.EndTagOpen:
                    return ProcessEndTagOpenState(currentElement);

                case HtmlStates.MarkupDeclarationOpen:
                    return ProcessMarkupDeclarationOpenState(currentElement);
            }
            throw new NotImplementedException("Unsupported html parser state " + _characterStream.State);
        }

        private Element ProcessDataState(Element currentElement)
        {
            var skipWhitespace = false;

            using (var text = _stringBuilderFactory.Create())
            {
                Func<IStringBuilder, bool> save = sb =>
                    {
                        if (currentElement != null && !currentElement.SuppressOutput)
                        {
                            var s = sb.ToString();
                            if (!string.IsNullOrWhiteSpace(s))
                            {
                                if (currentElement.Children == null)
                                    currentElement.Children = new List<IDocumentElement>();
                                var rawText = new RawTextElement { Parent = currentElement, Text = s };
                                currentElement.Children.Add(rawText);
                                if (!Begin(rawText)) return false;
                            }
                        }
                        return true;
                    };

                while (_characterStream.MoveNext())
                {
                    if (_characterStream.Current == '<')
                    {
                        if (_characterStream.CurrentRawInput == "<")
                        {
                            if (!save(text)) return null; ;
                            _characterStream.State = HtmlStates.TagOpen;
                            return currentElement;
                        }
                    }

                    var c = _characterStream.Current;
                    if (char.IsWhiteSpace(c))
                    {
                        if (!skipWhitespace)
                            text.Append(' ');
                        skipWhitespace = true;
                    }
                    else
                    {
                        text.Append(c);
                        skipWhitespace = false;
                    }
                }

                save(text);
                return null;
            }
        }

        /// <summary>
        /// Parses an opening tag like '&lt;div>'. Starts with the input stream
        /// pointing to the opening &lt; character.
        /// </summary>
        private Element ProcessTagOpenState(Element currentElement)
        {
            if (!_characterStream.MoveNext())
            {
                _document.ConformanceLevel *= 0.8f;
                return null;
            }

            switch (_characterStream.Current)
            {
                case '!':
                    _characterStream.State = HtmlStates.MarkupDeclarationOpen;
                    return currentElement;
                case '/':
                    _characterStream.State = HtmlStates.EndTagOpen;
                    return currentElement;
            }

            var startPosition = _characterStream.CurrentPosition;

            string tagName = null;
            var selfClosing = false;

            using (var nameBuffer = _stringBuilderFactory.Create(_maximumNameLength))
            {
                char? terminator;
                _characterStream.State = HtmlStates.TagName;
                if (_stringParser.TakeUntil(nameBuffer, _maximumNameLength, c => char.IsWhiteSpace(c) || c == '>' || c == '/', out terminator))
                {
                    tagName = nameBuffer.ToString().ToLower();
                    if (!terminator.HasValue || terminator == '/')
                    {
                        _characterStream.State = HtmlStates.SelfClosingStartTag;
                        if (_stringParser.Peek() == '>')
                            _stringParser.TakeOne();
                        else
                            _document.ConformanceLevel *= 0.9f;
                    }
                    else if (terminator == '>')
                    {
                        _characterStream.State = _voidElements.Contains(tagName)
                            ? HtmlStates.SelfClosingStartTag
                            : HtmlStates.Data;
                    }
                    else
                    {
                        _characterStream.State = HtmlStates.AttributeName;
                        selfClosing = _voidElements.Contains(tagName);
                    }
                }
                else
                {
                    _document.ConformanceLevel *= 0.9f;
                    nameBuffer.Clear();
                    _stringParser.Take(nameBuffer, _maximumNameLength);
                    var buffer = nameBuffer.ToString().ToLower();
                    _characterStream.State = HtmlStates.Data;
                    foreach (var name in _allElements)
                    {
                        if (buffer.StartsWith(name))
                        {
                            nameBuffer.Clear();
                            _characterStream.Reset(startPosition);
                            _stringParser.Take(nameBuffer, name.Length);
                            tagName = nameBuffer.ToString();
                            _characterStream.State = HtmlStates.AttributeName;
                            selfClosing = _voidElements.Contains(tagName);
                            break;
                        }
                    }
                    if (tagName == null)
                        _document.ConformanceLevel *= 0.6f;
                }
            }

            var attributes = _characterStream.State == HtmlStates.AttributeName ? ParseAttributes(selfClosing) : null;

            if (currentElement != null)
            {
                var parentElement = currentElement;

                switch (tagName)
                { 
                    case "html":
                    case "body":
                    case "form":
                    case "header":
                    case "footer":
                        // These elements are not parsed and contain no details. They are included in the output 
                        // only as containers for their children
                        currentElement = new UnsupportedElement { Attributes = attributes, SuppressOutput = false };
                        break;

                    case "p":
                    case "li":
                        // These elements are treated as paragraphs in other markup formats, for
                        // example in markdown they will have a blank line above them to create a
                        // paragraph break.
                        currentElement = new ParagraphElement { Attributes = attributes };
                        break;

                    case "blockquote":
                        currentElement = new ParagraphElement
                        {
                            Attributes = attributes,
                            Styles = new Dictionary<string, string> 
                            { 
                                { "margin-top", "10px" } ,
                                { "margin-bottom", "10px" } ,
                                { "margin-left", "50px" } ,
                                { "padding-left", "15px" } ,
                                { "border-left", "3px solid #ccc" }
                            }
                        };
                        break;

                    case "div":
                        // Divs are tricky because some pwople use them to group elements with similar
                        // style and other people used them instead of paragraphs. Since divs are by
                        // default block elements it makes more sense in most cases to treat them link
                        // paragraphs unless they have paraphraphs or other divs within them.
                        currentElement = new ContainerElement { ContainerType = ContainerTypes.Division, Attributes = attributes };
                        break;

                    case "span":
                        // These elements are treated as inline text. For example in markdown
                        // these are rendered without an extra blank line and are therefore rendered
                        // as part of the prior paragraph
                        currentElement = new SpanElement { Attributes = attributes };
                        break;

                    case "a":
                        // Anchor tags are a special case
                        if (attributes != null && attributes.ContainsKey("href"))
                            currentElement = new AnchorElement { LinkAddress = attributes["href"] };
                        else
                            currentElement = new UnsupportedElement { Attributes = attributes };
                        break;

                    case "iframe":
                    case "img":
                        // Image tags are a special case
                        if (attributes != null && attributes.ContainsKey("src"))
                        {
                            var alt = attributes.ContainsKey("alt") ? attributes["alt"] : null;
                            currentElement = new ImageElement { LinkAddress = attributes["src"], AltText = alt};
                        }
                        else
                        {
                            currentElement = new UnsupportedElement { Attributes = attributes };
                        }
                        break;

                    case "h1":
                        currentElement = new HeadingElement { Level = 1 };
                        break;
                    case "h2":
                        currentElement = new HeadingElement { Level = 2 };
                        break;
                    case "h3":
                        currentElement = new HeadingElement { Level = 3 };
                        break;
                    case "h4":
                        currentElement = new HeadingElement { Level = 4 };
                        break;
                    case "h5":
                        currentElement = new HeadingElement { Level = 5 };
                        break;
                    case "h6":
                        currentElement = new HeadingElement { Level = 6 };
                        break;

                    case "strong":
                    case "b":
                        // Bold is represented as an inline style
                        currentElement = new FormattedElement 
                        {
                            ElementType = ElementTypes.InlineText, 
                            Styles = new Dictionary<string, string> 
                            { {"font-weight", "bold"}} 
                        };
                        break;

                    case "cite":
                    case "q":
                    case "i":
                    case "em":
                        // Italic is represented as an inline style
                        currentElement = new FormattedElement 
                        {
                            ElementType = ElementTypes.InlineText, 
                            Styles = new Dictionary<string, string> 
                            { { "font-style", "italic" } } 
                        };
                        break;

                    case "u":
                        // Underline is represented as an inline style
                        currentElement = new FormattedElement
                        {
                            ElementType = ElementTypes.InlineText,
                            Styles = new Dictionary<string, string> 
                            { { "text-decoration", "underline" } }
                        };
                        break;

                    case "small":
                        // Small is represented as an inline style
                        currentElement = new FormattedElement 
                        {
                            ElementType = ElementTypes.InlineText,
                            Styles = new Dictionary<string, string> 
                            { { "font-size", "smaller" } } 
                        };
                        break;

                    case "sup":
                        // Superscript is represented as an inline style
                        currentElement = new FormattedElement
                        {
                            ElementType = ElementTypes.InlineText,
                            Styles = new Dictionary<string, string> 
                            { 
                                { "vertical-align", "super" }, 
                                { "font-size", "smaller" } 
                            }
                        };
                        break;

                    case "sub":
                        // Subscript is represented as an inline style
                        currentElement = new FormattedElement
                        {
                            ElementType = ElementTypes.InlineText,
                            Styles = new Dictionary<string, string> 
                            { 
                                { "vertical-align", "sub" },
                                { "font-size", "smaller" }
                            }
                        };
                        break;

                    case "br":
                        currentElement = new BreakElement { BreakType = BreakTypes.LineBreak };
                        break;

                    case "hr":
                        currentElement = new BreakElement { BreakType = BreakTypes.HorizontalRule };
                        break;

                    case "ul":
                        currentElement = new ContainerElement { ContainerType = ContainerTypes.BulletList, Attributes = attributes };
                        break;

                    case "ol":
                        currentElement = new ContainerElement { ContainerType = ContainerTypes.NumberedList, Attributes = attributes };
                        break;

                    case "table":
                        currentElement = new ContainerElement { ContainerType = ContainerTypes.Table, Attributes = attributes };
                        break;

                    case "tr":
                        currentElement = new ContainerElement { ContainerType = ContainerTypes.TableDataRow, Attributes = attributes };
                        break;

                    case "th":
                        currentElement = new ContainerElement { ContainerType = ContainerTypes.TableHeaderRow, Attributes = attributes };
                        break;

                    case "td":
                        currentElement = new ContainerElement { ContainerType = ContainerTypes.TableDataCell, Attributes = attributes };
                        break;

                    default:
                        // All other elements will be excluded from the output document, but will
                        // be parsed just so that we know where they and and the next valid element
                        // begins.
                        currentElement = new UnsupportedElement { Attributes = attributes };
                        break;
                }

                var styleElement = currentElement as IStyleElement;
                if (styleElement != null && attributes != null)
                {
                    if (attributes.ContainsKey("class"))
                    {
                        styleElement.ClassNames = attributes["class"];
                        attributes.Remove("class");
                    }
                    if (attributes.ContainsKey("style"))
                    {
                        if (styleElement.Styles == null)
                            styleElement.Styles = new Dictionary<string, string>();
                        var styles = attributes["style"].Split(';').Select(s => s.Trim()).Where(s => s.Length > 0);
                        foreach (var style in styles)
                        {
                            var colonPos = style.IndexOf(':');
                            if (colonPos > 0 && colonPos < style.Length - 1)
                            {
                                var name = style.Substring(0, colonPos).Trim().ToLower();
                                var value = style.Substring(colonPos + 1).Trim().ToLower();
                                if (!styleElement.Styles.ContainsKey(name))
                                    styleElement.Styles[name] = value;
                            }
                            else
                            {
                                _document.ConformanceLevel *= 0.9f;
                            }
                        }
                        attributes.Remove("style");
                    }
                }

                currentElement.Name = tagName;
                currentElement.Parent = parentElement;
                if (parentElement.SuppressOutput) currentElement.SuppressOutput = true;

                if (parentElement.Children == null)
                    parentElement.Children = new List<IDocumentElement>();
                parentElement.Children.Add(currentElement);

                if (!Begin(currentElement)) return null;
            }

            return currentElement;
        }

        private IDictionary<string, string> ParseAttributes(bool selfClosing)
        {
            var attributes = new Dictionary<string, string>();

            using (var nameBuffer = _stringBuilderFactory.Create(_maximumNameLength))
            {
                using (var valueBuffer = _stringBuilderFactory.Create(_maximumAttributeValueLength))
                { 
                    _characterStream.State = HtmlStates.BeforeAttributeName;
                    var attributeName = string.Empty;

                    while (true)
                    {
                        char? terminator;
                        switch (_characterStream.State)
                        {
                            case HtmlStates.BeforeAttributeName:
                                nameBuffer.Clear();
                                _stringParser.SkipWhitespace();
                                _stringParser.TakeUntil(
                                    nameBuffer, 
                                    _maximumNameLength, 
                                    c => char.IsWhiteSpace(c) || c == '=' || c == '\'' || c == '"' || c == '/' || c == '>', 
                                    out terminator);
                                if (terminator == '/')
                                {
                                    _characterStream.State = HtmlStates.SelfClosingStartTag;
                                    if (_characterStream.Peek() == '>')
                                        _characterStream.MoveNext();
                                    else
                                        _document.ConformanceLevel *= 0.95f;
                                    return attributes;
                                }
                                if (terminator == '>')
                                {
                                    _characterStream.State = selfClosing ? HtmlStates.SelfClosingStartTag : HtmlStates.Data;
                                    return attributes;
                                }
                                if (terminator == '\'')
                                {
                                    attributeName = nameBuffer.ToString().ToLower();
                                    _characterStream.State = HtmlStates.AttributeValueSingleQuoted;
                                }
                                else if (terminator == '"')
                                {
                                    attributeName = nameBuffer.ToString().ToLower();
                                    _characterStream.State = HtmlStates.AttributeValueDoubleQuoted;
                                }
                                else
                                {
                                    attributeName = nameBuffer.ToString().ToLower();
                                    _characterStream.State = HtmlStates.AfterAttributeName;
                                }
                                break;

                            case HtmlStates.AfterAttributeName:
                                _stringParser.SkipWhitespace();
                                if (_characterStream.Current == '=')
                                {
                                    _characterStream.MoveNext();
                                }
                                else if (_characterStream.Current == '\'')
                                {
                                    _characterStream.State = HtmlStates.AttributeValueSingleQuoted;
                                }
                                else if (_characterStream.Current == '"')
                                {
                                    _characterStream.State = HtmlStates.AttributeValueDoubleQuoted;
                                }
                                else if (_characterStream.Current == '/')
                                {
                                    attributes[attributeName] = string.Empty;
                                    _characterStream.State = HtmlStates.SelfClosingStartTag;
                                    if (_characterStream.Peek() == '>') 
                                        _characterStream.MoveNext();
                                    else
                                        _document.ConformanceLevel *= 0.9f;
                                    return attributes;
                                }
                                else if (_characterStream.Current == '>')
                                {
                                    attributes[attributeName] = string.Empty;
                                    _characterStream.State = selfClosing ? HtmlStates.SelfClosingStartTag : HtmlStates.Data;
                                    return attributes;
                                }
                                else
                                {
                                    valueBuffer.Clear();
                                    _characterStream.State = HtmlStates.AttributeValueUnquoted;
                                    if (_stringParser.TakeUntil(
                                        valueBuffer,
                                        _maximumAttributeValueLength,
                                        c => char.IsWhiteSpace(c) || c == '/' || c == '>' || c == '\'' || c == '"',
                                        out terminator))
                                    {
                                        attributes[attributeName] = valueBuffer.ToString();
                                        _characterStream.State = HtmlStates.BeforeAttributeName;

                                        if (terminator == '/')
                                        {
                                            _document.ConformanceLevel *= 0.9f;
                                            _characterStream.State = HtmlStates.SelfClosingStartTag;
                                            if (_characterStream.Peek() == '>') _characterStream.MoveNext();
                                            return attributes;
                                        }
                                        if (terminator == '>')
                                        {
                                            _document.ConformanceLevel *= 0.9f;
                                            _characterStream.State = selfClosing ? HtmlStates.SelfClosingStartTag : HtmlStates.Data;
                                            return attributes;
                                        }
                                    }
                                    else
                                        throw new Exception("Unable to parse HTML, attribute value is unterminated or extremely long");
                                }
                                break;

                            case HtmlStates.AttributeValueSingleQuoted:
                            case HtmlStates.AttributeValueDoubleQuoted:
                                var closingQuote = _characterStream.State == HtmlStates.AttributeValueSingleQuoted ? '\'' : '"';
                                valueBuffer.Clear();
                                if (_stringParser.TakeUntil(valueBuffer, _maximumAttributeValueLength, closingQuote, false))
                                {
                                    attributes[attributeName] = valueBuffer.ToString();
                                    _characterStream.MoveNext();
                                    _characterStream.State = HtmlStates.BeforeAttributeName;
                                }
                                else if (_stringParser.TakeUntil(valueBuffer, 0, '>', false))
                                {
                                    attributes[attributeName] = valueBuffer.ToString();
                                    _characterStream.State = selfClosing ? HtmlStates.SelfClosingStartTag : HtmlStates.Data;
                                    return attributes;
                                }
                                else
                                {
                                    _document.ConformanceLevel = 0f;
                                    throw new Exception("Unable to parse HTML, attribute value is unterminated or extremely long");
                                }
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Transitions back into data state after a self-closing tag
        /// </summary>
        private Element ProcessSelfClosingStartTagState(Element currentElement)
        {
            _characterStream.State = HtmlStates.Data;

            if (currentElement == null)
                return null;

            return End(currentElement) ? (Element)currentElement.Parent : null;
        }

        /// <summary>
        /// Parses closing tags (like '&lt;/h3>'). Starts with the input stream pointing to the '/' of the
        /// closing tag and ends with the imput stream pointing to the first character after the closing '>'
        /// </summary>
        private Element ProcessEndTagOpenState(Element currentElement)
        {
            var startPosition = _characterStream.CurrentPosition;
            _characterStream.State = HtmlStates.TagName;

            string tagName = null;
            using (var nameBuffer = _stringBuilderFactory.Create(_maximumNameLength))
            {
                char? terminator;
                if (_stringParser.TakeUntil(nameBuffer, _maximumNameLength, c => char.IsWhiteSpace(c) || c == '>', out terminator, false))
                {
                    tagName = nameBuffer.ToString().ToLower();
                    if (terminator.HasValue && terminator.Value != '>')
                    {
                        _stringParser.SkipUntil(_maximumNameLength, c => c == '>' || !(c == '/' || char.IsWhiteSpace(c)), out terminator, false);
                        if (terminator == '>') _stringParser.TakeOne();
                    }
                }
                else
                {
                    nameBuffer.Clear();
                    _stringParser.Take(nameBuffer, _maximumNameLength);
                    var buffer = nameBuffer.ToString().ToLower();
                    foreach (var name in _allElements)
                    {
                        if (buffer.StartsWith(name))
                        {
                            nameBuffer.Clear();
                            _characterStream.Reset(startPosition);
                            _stringParser.Take(nameBuffer, name.Length);
                            tagName = nameBuffer.ToString();
                            break;
                        }
                    }
                }
            }

            _characterStream.State = HtmlStates.Data;

            if (currentElement == null)
                return null;

            var hasOpeningTag = false;
            {
                var e = currentElement;
                while (!hasOpeningTag && e != null)
                {
                    hasOpeningTag = e.Name == tagName;
                    e = (Element)e.Parent;
                }
            }

            if (hasOpeningTag)
            {
                bool matchingTagName;
                do
                {
                    matchingTagName = currentElement.Name == tagName;
                    End(currentElement);
                    currentElement = (Element)currentElement.Parent;
                } while (currentElement != null && !matchingTagName);
            }

            return currentElement;
        }

        private Element ProcessMarkupDeclarationOpenState(Element currentElement)
        {
            using (var buffer  = _stringBuilderFactory.Create())
            {
                char? terminator;
                var startPosition = _characterStream.CurrentPosition;
                if (_stringParser.Take(buffer, 7, false))
                {
                    _characterStream.Reset(startPosition);
                    var prefix = buffer.ToString();

                    if (prefix[0] == '-' && prefix[1] == '-')
                    {
                        _characterStream.State = HtmlStates.Comment;
                        _stringParser.Skip(2);
                        while (!_characterStream.Eof)
                        {
                            _stringParser.SkipUntil(0, c => c == '-', out terminator);
                            while (_stringParser.Peek() == '-')
                            {
                                _stringParser.Skip(1);
                                if (_stringParser.Peek() == '>')
                                {
                                    _stringParser.Skip(1);
                                    _characterStream.State = HtmlStates.Data;
                                    return currentElement;
                                }
                            }
                        }
                        _characterStream.State = HtmlStates.Data;
                        return currentElement;
                    }

                    if (prefix.StartsWith("DOCTYPE"))
                    {
                        _characterStream.State = HtmlStates.DocType;
                        _stringParser.SkipUntil(0, c => c == '>', out terminator);
                        _characterStream.State = HtmlStates.Data;
                        return currentElement;
                    }

                    if (prefix.StartsWith("[CDATA["))
                    {
                        _stringParser.Skip(7);
                        _characterStream.State = HtmlStates.CDataSection;
                        while (!_characterStream.Eof)
                        {
                            _stringParser.SkipUntil(0, c => c == ']', out terminator);
                            while (_stringParser.Peek() == ']')
                            {
                                _stringParser.Skip(1);
                                if (_stringParser.Peek() == '>')
                                {
                                    _stringParser.Skip(1);
                                    _characterStream.State = HtmlStates.Data;
                                    return currentElement;
                                }
                            }
                        }
                    }

                    _characterStream.State = HtmlStates.BogusDocType;
                    _stringParser.SkipUntil(0, c => c == '>', out terminator);
                }
            }

            _characterStream.State = HtmlStates.Data;
            return currentElement;
        }
        
        #region Static data

        private readonly List<string> _voidElements = new List<string> 
        { 
            "area", "base", "br", "col", "embed", "hr", "img", "input", "keygen", 
            "link", "menuitem", "meta", "param", "source", "track", "wbr" 
        };

        private readonly List<string> _rawElements = new List<string> 
        { 
            "script", "style" 
        };

        private readonly List<string> _escapableRawElements = new List<string> 
        { 
            "textarea", "title" 
        };

        private readonly List<string> _normalElements = new List<string> 
        { 
            "html", "head", "body", "article", "section", "nav", "aside", 
            "h1", "h2", "h3", "h4", "h5", "h6", "header", "footer", "address",
            "p", "pre", "blockquote", "ol", "ul", "li", "dl", "dt", "dd", "figure", 
            "figcaption", "div", "main", "a", "em", "strong", "small", "s", "cite", "q", 
            "dfn", "abbr", "data", "time", "code", "var", "samp", "kbd", "sub", "sup", 
            "i", "b", "u", "mark", "ruby", "rb", "rt", "rtc", "rp", "bdi", "bdo", "span", 
            "ins", "del", "iframe", "object", "param", "video", "audio", "map", "area", 
            "a", "alternate", "author", "bookmark", "help", "icon", "license", "nofollow", 
            "noreferrer", "prefetch", "search", "stylesheet", "tag", "next", "prev", 
            "table", "caption", "colgroup", "col", "tbody", "thread", "tfoot", "tr", 
            "td", "th", "form", "label", "button", "select", "datalist", "optgroup", 
            "option", "output", "progress", "meter", "fieldset", "legend", "script", 
            "noscript", "template", "canvas"
        };

        private readonly List<string> _allElements;

        #endregion
    }
}