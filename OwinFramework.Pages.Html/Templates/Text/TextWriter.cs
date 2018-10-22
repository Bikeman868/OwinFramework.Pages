namespace OwinFramework.Pages.Html.Templates.Text
{
    internal class TextWriter
    {
        private readonly ICharacterStreamWriter _characterStream;

        public string LinePrefix { get; set; }

        private int _lineLength;
        private bool _hasBlankLine = true;

        public TextWriter(
            ICharacterStreamWriter characterStream)
        {
            _characterStream = characterStream;
        }

        public void WriteLineBreak()
        {
            if (_lineLength == 0) _hasBlankLine = true;
            _characterStream.WriteLineBreak();
            _lineLength = 0;
        }

        public void EnsureNewLine()
        {
            if (_lineLength > 0)
                WriteLineBreak();
        }

        public void EnsureBlankLine()
        {
            while (!_hasBlankLine)
                WriteLineBreak();
        }

        public void Write(char c)
        {
            if (c == '\r' || c == '\n')
            {
                EnsureNewLine();
            }
            else
            {
                if (_lineLength == 0 && LinePrefix != null)
                {
                    _characterStream.Write(LinePrefix);
                    _lineLength += LinePrefix.Length;
                }
                _characterStream.Write(c);
                _lineLength++;
                _hasBlankLine = false;
            }
        }

        public void Write(string s)
        {
            if (s != null)
                foreach (var c in s)
                    Write(c);
        }

        public void WriteQuotedString(string s)
        {
            if (_lineLength == 0 && LinePrefix != null)
            {
                _characterStream.Write(LinePrefix);
                _lineLength += LinePrefix.Length;
            }
            _characterStream.WriteQuotedString(s);
            _lineLength += s.Length + 2;
        }

        public void WriteBlockComment(string s)
        {
            EnsureNewLine();
            _characterStream.WriteBlockComment(s);
            _hasBlankLine = false;
        }

    }
}
