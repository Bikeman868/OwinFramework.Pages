using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Facilities.Runtime
{
    /// <summary>
    /// A TextWriter that writes HTML to the response. Has special features for
    /// writing HTML elements and also for allowing multiple threads to simultaneously
    /// write into different parts of the output buffer.
    /// </summary>
    public class HtmlWriter: TextWriter, IHtmlWriter
    {
        private class BufferListElement
        {
            private StringBuilder _buffer = new StringBuilder();
            public BufferListElement Next;

            public BufferListElement InsertAfter()
            {
                lock (this)
                {
                    var result = new BufferListElement
                    {
                        _buffer = new StringBuilder(),
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

            public override string ToString()
            {
                return _buffer.ToString();
            }
        }

        /// <summary>
        /// Returns the encoding to use for this TextWriter
        /// </summary>
        public override Encoding Encoding { get { return Encoding.UTF8; } }

        /// <summary>
        /// The current indentation level
        /// </summary>
        public int IndentLevel { get; set; }

        private readonly BufferListElement _bufferListHead;
        private BufferListElement _bufferListTail;
        private bool _startOfLine;

        /// <summary>
        /// Constructs a new HTML Writer
        /// </summary>
        public HtmlWriter()
        {
            _startOfLine = true;
            IndentLevel = 0;

            _bufferListHead = new BufferListElement();
            _bufferListTail = _bufferListHead;
        }

        private HtmlWriter(HtmlWriter parent)
        {
            _startOfLine = parent._startOfLine;
            IndentLevel = parent.IndentLevel;

            _bufferListHead = parent._bufferListTail;
            _bufferListTail = _bufferListHead;
        }

        /// <summary>
        /// Constructs and returns an HtmlWriter that will insert into the
        /// output buffer at the current spot. You can use this to start
        /// an async process that will insert text into the output when it
        /// completes.
        /// </summary>
        public IHtmlWriter CreateInsertionPoint()
        {
            var result = new HtmlWriter(this);

            _bufferListTail = _bufferListTail.InsertAfter();

            return result;
        }

        /// <summary>
        /// Writes the buffered text to the response. Make sure all threads
        /// have finished writing before calling this method
        /// </summary>
        /// <param name="context">The owin context of the response to write</param>
        public void ToResponse(IOwinContext context)
        {
            var buffer = _bufferListHead;
            while (buffer != null)
            {
                context.Response.Write(buffer.ToString());
                buffer = buffer.Next;
            }
        }

        /// <summary>
        /// Returns a task that will asynchronously write the response
        /// </summary>
        /// <param name="context">The owin context of the response to write</param>
        public Task ToResponseAsync(IOwinContext context)
        {
            return Task.Factory.StartNew(() => ToResponse(context));
        }

        /// <summary>
        /// Writes a single character to the response buffer
        /// </summary>
        public override void Write(char c)
        {
            if (_startOfLine)
            {
                if (IndentLevel > 0)
                {
                    for (var i = 0; i < IndentLevel; i++)
                    {
                        _bufferListTail.Write(' ');
                        _bufferListTail.Write(' ');
                        _bufferListTail.Write(' ');
                    }
                }
                _startOfLine = false;
            }
            _bufferListTail.Write(c);
        }

        public override void WriteLine()
        {
            base.WriteLine();
            _startOfLine = true;
        }

        /// <summary>
        /// Writes the openinf tag of an html element
        /// </summary>
        /// <param name="tag">The html tag to write</param>
        /// <param name="selfClosing">Pass true to self close the element</param>
        /// <param name="attributePairs">Name value pairs of the element attributes</param>
        public IHtmlWriter WriteOpenTag(string tag, bool selfClosing, params string[] attributePairs)
        {
            Write('<');
            Write(tag);

            WriteAttributes(attributePairs);

            if (selfClosing)
            {
                Write(" />");
            }
            else
            {
                WriteLine('>');
                IndentLevel++;
            }

            return this;
        }

        private void WriteAttributes(string[] attributePairs)
        {
            if (attributePairs != null)
            {
                for (var i = 0; i < attributePairs.Length; i += 2)
                {
                    Write(' ');
                    Write(attributePairs[i]);
                    Write("='");
                    Write(attributePairs[i + 1]);
                    Write('\'');
                }
            }
        }

        /// <summary>
        /// Writes the opening tag of an html element that contains other elements
        /// You must close this element after writing the contents
        /// </summary>
        /// <param name="tag">The html tag to write</param>
        /// <param name="attributePairs">Name value pairs of the element attributes</param>
        public IHtmlWriter WriteOpenTag(string tag, params string[] attributePairs)
        {
            return WriteOpenTag(tag, false, attributePairs);
        }

        /// <summary>
        /// Writes the closing tag of an element
        /// </summary>
        /// <param name="tag">The element tag to close</param>
        public IHtmlWriter WriteCloseTag(string tag)
        {
            IndentLevel--;
            Write("</");
            Write(tag);
            Write('>');
            WriteLine();
            return this;
        }

        /// <summary>
        /// Writes a simple html element with an opening and closing tag and some
        /// content in between
        /// </summary>
        /// <param name="tag">The tag to write</param>
        /// <param name="content">The content inside the element</param>
        /// <param name="attributePairs">Attributes to apply to the opening tag</param>
        public IHtmlWriter WriteElement(string tag, string content, params string[] attributePairs)
        {
            Write('<');
            Write(tag);
            WriteAttributes(attributePairs);
            Write('>');
            Write(content);
            Write("</");
            Write(tag);
            Write('>');

            return this;
        }

        #region IHtmlWriter

        IHtmlWriter IHtmlWriter.Write(char c)
        {
            Write(c);
            return this;
        }

        public TextWriter GetTextWriter()
        {
            return this;
        }

        IHtmlWriter IHtmlWriter.Write(string s)
        {
            Write(s);
            return this;
        }

        IHtmlWriter IHtmlWriter.Write<T>(T s)
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

        IHtmlWriter IHtmlWriter.WriteLine<T>(T s)
        {
            WriteLine(s);
            return this;
        }

        #endregion

    }
}
