using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Templates.Text;
using TextWriter = System.IO.TextWriter;

namespace OwinFramework.Pages.Html.Runtime
{
    /// <summary>
    /// A TextWriter that writes HTML to the response. Has special features for
    /// writing HTML elements and also for allowing multiple threads to simultaneously
    /// write into different parts of the output buffer.
    /// </summary>
    public class HtmlWriter: TextWriter, IHtmlWriter
    {
        /// <summary>
        /// Returns the encoding to use for this TextWriter
        /// </summary>
        public override Encoding Encoding { get { return Encoding.UTF8; } }

        /// <summary>
        /// The current indentation level
        /// </summary>
        public int IndentLevel { get; set; }

        /// <summary>
        /// Defines the Html standards to apply to the Html produced
        /// </summary>
        public HtmlFormat Format { get; set; }

        /// <summary>
        /// Turns indentation on/off. The html is more readable with
        /// indentation turned on but the output is bigger because of 
        /// all the extra spaces
        /// </summary>
        public bool Indented { get; set; }

        /// <summary>
        /// Turns comments on/off. The comments are good for debugging 
        /// issues but the html can be a lot larger
        /// </summary>
        public bool IncludeComments { get; set; }

        private readonly IStringBuilderFactory _stringBuilderFactory;
        private readonly bool _isBufferOwner;
        private readonly HtmlCharacterStream _characterStream;
        private BufferListElement _bufferListHead;
        private BufferListElement _bufferListTail;
        private bool _startOfLine;

        /// <summary>
        /// Constructs a new HTML Writer
        /// </summary>
        public HtmlWriter(
            IStringBuilderFactory stringBuilderFactory,
            IFrameworkConfiguration frameworkConfiguration)
        {
            _stringBuilderFactory = stringBuilderFactory;
            _isBufferOwner = true;
            _startOfLine = true;

            Indented = frameworkConfiguration.Indented;
            IncludeComments = frameworkConfiguration.IncludeComments;
            Format = frameworkConfiguration.HtmlFormat;
            IndentLevel = 0;

            _bufferListHead = new BufferListElement(stringBuilderFactory);
            _bufferListTail = _bufferListHead;
            _characterStream = new HtmlCharacterStream(this);
        }

        private HtmlWriter(HtmlWriter parent)
        {
            _stringBuilderFactory = parent._stringBuilderFactory;
            _isBufferOwner = false;
            _startOfLine = parent._startOfLine;
            Indented = parent.Indented;
            IndentLevel = parent.IndentLevel;

            _bufferListHead = parent._bufferListTail;
            _bufferListTail = _bufferListHead;
        }

        #region Writing directly to the output buffer

        private void WriteRaw(char c)
        {
            _bufferListTail.Write(c);
        }

        private void WriteRaw(string s)
        {
            _bufferListTail.Write(s);
        }

        #endregion

        #region Overriding TextWriter with indentation

        private void WriteIndent()
        {
            if (_startOfLine)
            {
                if (Indented && IndentLevel > 0)
                {
                    for (var i = 0; i < IndentLevel; i++)
                        WriteRaw("   ");
                }
                _startOfLine = false;
            }
        }

        public override void Write(char c)
        {
            if (_startOfLine) WriteIndent();
            WriteRaw(c);
        }

        public override void WriteLine()
        {
            WriteRaw('\n');
            _startOfLine = true;
        }

        public override void WriteLine(string s)
        {
            if (_startOfLine && !Indented)
                s = s.Trim();

            WriteIndent();

            WriteRaw(s);
            WriteRaw('\n');

            _startOfLine = true;
        }

        #endregion

        #region Low level methods to write bits of the Html syntax

        private void WriteOpenStart(string tag)
        {
            Write('<');

            _characterStream.State = HtmlStates.TagName;
            _characterStream.Write(tag);
        }

        private void WriteOpenAttributes(params string[] attributePairs)
        {
            if (attributePairs != null)
            {
                for (var i = 0; i < attributePairs.Length; i += 2)
                {
                    Write(' ');

                    _characterStream.State = HtmlStates.AttributeName;
                    _characterStream.Write(attributePairs[i]);

                    Write('=');

                    _characterStream.WriteQuotedString(attributePairs[i + 1]);
                    _characterStream.State = HtmlStates.AfterAttributeValueQuoted;
                }
            }
        }

        private void WriteOpenSelfClose()
        {
            Write(" />");
        }

        private void WriteOpenEnd()
        {
            Write('>');
        }

        private void WriteClose(string tag)
        {
            Write("</");

            _characterStream.State = HtmlStates.TagName;
            _characterStream.Write(tag);

            Write(">");
        }

        #endregion

        #region Buffer list

        private class BufferListElement: IDisposable
        {
            private readonly IStringBuilderFactory _stringBuilderFactory;
            private readonly IStringBuilder _buffer;
            public BufferListElement Next;

            public BufferListElement(
                IStringBuilderFactory stringBuilderFactory)
            {
                _stringBuilderFactory = stringBuilderFactory;
                _buffer = stringBuilderFactory.Create();
            }

            public void Dispose()
            {
                _buffer.Dispose();

                var next = Next;
                Next = null;

                if (next != null)
                    next.Dispose();
            }

            public BufferListElement InsertAfter()
            {
                lock (this)
                {
                    var result = new BufferListElement(_stringBuilderFactory)
                    {
                        Next = Next,
                    };
                    Next = result;
                    return result;
                }
            }

            public void Write(char c)
            {
                _buffer.Append(c);
            }

            public void Write(string s)
            {
                _buffer.Append(s);
            }

            public override string ToString()
            {
                return _buffer.ToString();
            }
        }

        #endregion

        #region IHtmlWriter

        void IHtmlWriter.ToResponse(IOwinContext context)
        {
            var buffer = _bufferListHead;
            while (buffer != null)
            {
                context.Response.Write(buffer.ToString());
                buffer = buffer.Next;
            }

            if (_isBufferOwner && _bufferListHead != null)
                _bufferListHead.Dispose();

            _bufferListHead = null;
            _bufferListTail = null;
        }

        Task IHtmlWriter.ToResponseAsync(IOwinContext context)
        {
            return Task.Factory.StartNew(() => ((IHtmlWriter)this).ToResponse(context));
        }

        void IHtmlWriter.ToStringBuilder(IStringBuilder stringBuilder)
        {
            var buffer = _bufferListHead;
            while (buffer != null)
            {
                stringBuilder.Append(buffer.ToString());
                buffer = buffer.Next;
            }

            if (_isBufferOwner && _bufferListHead != null)
                _bufferListHead.Dispose();

            _bufferListHead = null;
            _bufferListTail = null;
        }

        TextWriter IHtmlWriter.GetTextWriter()
        {
            return this;
        }

        IHtmlWriter IHtmlWriter.CreateInsertionPoint()
        {
            var result = new HtmlWriter(this);

            _bufferListTail = _bufferListTail.InsertAfter();

            return result;
        }

        IHtmlWriter IHtmlWriter.Write(char c)
        {
            Write(c);
            return this;
        }

        IHtmlWriter IHtmlWriter.Write(string s)
        {
            Write(s);
            return this;
        }

        IHtmlWriter IHtmlWriter.WriteLine()
        {
            WriteLine();
            return this;
        }

        IHtmlWriter IHtmlWriter.WriteLine(string s)
        {
            WriteLine(s);
            return this;
        }

        IHtmlWriter IHtmlWriter.WriteText(char c)
        {
            _characterStream.Write(c);
            return this;
        }

        IHtmlWriter IHtmlWriter.WriteText(string s)
        {
            _characterStream.Write(s);
            return this;
        }

        IHtmlWriter IHtmlWriter.WriteText<T>(T o)
        {
            _characterStream.Write(o.ToString());
            return this;
        }

        IHtmlWriter IHtmlWriter.WriteTextLine<T>(T o)
        {
            _characterStream.Write(o.ToString());
            _characterStream.WriteLineBreak();
            return this;
        }

        IHtmlWriter IHtmlWriter.WriteDocumentStart(string language)
        {
            if (Format == HtmlFormat.XHtml)
            {
                WriteLine("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">");
                ((IHtmlWriter)this).WriteOpenTag("html", "itemtype", "http://schema.org/WebPage", "lang", language, "xmlns", "http://www.w3.org/1999/xhtml");
                WriteLine();
            }
            else
            {
                WriteLine("<!DOCTYPE html>");
                ((IHtmlWriter)this).WriteOpenTag("html", "itemtype", "http://schema.org/WebPage", "lang", language);
                WriteLine();
            }

            return this;
        }

        IHtmlWriter IHtmlWriter.WriteDocumentEnd()
        {
            ((IHtmlWriter)this).WriteCloseTag("html");
            return this;
        }

        IHtmlWriter IHtmlWriter.WriteOpenTag(string tag, bool selfClosing, params string[] attributePairs)
        {
            WriteOpenStart(tag);
            WriteOpenAttributes(attributePairs);

            if (selfClosing)
            {
                WriteOpenSelfClose();
                _characterStream.State = HtmlStates.Data;
            }
            else
            {
                WriteOpenEnd();
                IndentLevel++;
                _characterStream.State = HtmlStates.PlainText;
            }

            return this;
        }

        IHtmlWriter IHtmlWriter.WriteOpenTag(string tag, params string[] attributePairs)
        {
            WriteOpenStart(tag);
            WriteOpenAttributes(attributePairs);
            WriteOpenEnd();

            IndentLevel++;

            _characterStream.State = HtmlStates.PlainText;
            return this;
        }

        IHtmlWriter IHtmlWriter.WriteCloseTag(string tag)
        {
            IndentLevel--;

            WriteClose(tag);

            _characterStream.State = HtmlStates.Data;
            return this;
        }

        IHtmlWriter IHtmlWriter.WriteElement(string tag, string content, params string[] attributePairs)
        {
            WriteOpenStart(tag);
            WriteOpenAttributes(attributePairs);
            WriteOpenEnd();

            if (!string.IsNullOrEmpty(content))
            {
                _characterStream.State = HtmlStates.PlainText;
                _characterStream.Write(content);
            }

            WriteClose(tag);

            _characterStream.State = HtmlStates.Data;
            return this;
        }

        IHtmlWriter IHtmlWriter.WriteElementLine(string tag, string content, params string[] attributePairs)
        {
            ((IHtmlWriter)this).WriteElement(tag, content, attributePairs);
            WriteLine();
            return this;
        }

        IHtmlWriter IHtmlWriter.WriteUnclosedElement(string tag, params string[] attributePairs)
        {
            if (Format == HtmlFormat.XHtml)
                return ((IHtmlWriter)this).WriteElement(tag, null, attributePairs);

            WriteOpenStart(tag);
            WriteOpenAttributes(attributePairs);
            WriteOpenEnd();

            _characterStream.State = HtmlStates.Data;
            return this;
        }

        IHtmlWriter IHtmlWriter.WriteComment(string comment, CommentStyle commentStyle)
        {
            if (IncludeComments && !string.IsNullOrEmpty(comment))
            {
                if (commentStyle == CommentStyle.Xml)
                {
                    _characterStream.WriteBlockComment(comment);
                    WriteLine();
                }
                else if (commentStyle == CommentStyle.SingleLineC)
                {
                    Write("// ");
                    WriteLine(comment);
                }
                else if (commentStyle == CommentStyle.MultiLineC)
                {
                    Write("/* ");
                    Write(comment);
                    WriteLine(" */");
                }
            }

            return this;
        }

        IHtmlWriter IHtmlWriter.WriteScriptOpen(string type)
        {
            ((IHtmlWriter)this).WriteOpenTag("script", "type", type);

            if (Format == HtmlFormat.XHtml)
            {
                WriteLine("//<![CDATA[");
                IndentLevel++;
            }

            return this;
        }

        IHtmlWriter IHtmlWriter.WriteScriptClose()
        {
            if (Format == HtmlFormat.XHtml)
            {
                IndentLevel--;
                WriteLine("//]]>");
            }

            ((IHtmlWriter)this).WriteCloseTag("script");

            return this;
        }

        IHtmlWriter IHtmlWriter.WritePreformatted(string text)
        {
            var wasIndented = Indented;
            var originalState = _characterStream.State;

            Indented = false;
            _characterStream.State = HtmlStates.Data;

            _characterStream.Write(text);

            Indented = wasIndented;
            _characterStream.State = originalState;

            return this;
        }

        #endregion
    }
}
