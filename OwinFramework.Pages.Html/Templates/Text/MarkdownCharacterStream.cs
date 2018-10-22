using System;
using System.Collections.Generic;
using System.IO;

namespace OwinFramework.Pages.Html.Templates.Text
{
    // See https://github.com/adam-p/markdown-here/wiki/Markdown-Cheatsheet

    internal class MarkdownCharacterStream: ICharacterStreamReader, ICharacterStreamWriter
    {
        private readonly string _buffer;
        private readonly System.IO.TextWriter _writer;

        private CharacterStreamPosition _currentPosition;

        public MarkdownStates State;

        public MarkdownCharacterStream(TextReader reader)
        {
            _buffer = reader.ReadToEnd();
            Reset();
        }

        public MarkdownCharacterStream(System.IO.TextWriter writer)
        {
            _writer = writer;
            State = MarkdownStates.ParagraphBreak;
        }

        void IDisposable.Dispose()
        {
        }

        #region ICharacterStreamReader

        public ICharacterStreamPosition CurrentPosition
        {
            get 
            { 
                return new CharacterStreamPosition 
                { 
                    Position = _currentPosition.Position,
                    InputLength = _currentPosition.InputLength,
                    Character = _currentPosition.Character,
                    BlankLines = _currentPosition.BlankLines
                }; 
            }
        }

        public void Reset(ICharacterStreamPosition position)
        {
            var p = (CharacterStreamPosition)position;
            _currentPosition.Position = p.Position;
            _currentPosition.InputLength = p.InputLength;
            _currentPosition.Character = p.Character;
            _currentPosition.BlankLines = p.BlankLines;
        }

        public char Current
        {
            get { return _currentPosition.Character; }
        }

        object System.Collections.IEnumerator.Current
        {
            get { return _currentPosition.Character; }
        }

        public string CurrentRawInput 
        { 
            get
            {
                return _buffer.Substring(
                    _currentPosition.Position - _currentPosition.InputLength + 1, 
                    _currentPosition.InputLength);
            }
        }

        public bool Eof
        {
            get { return _currentPosition.Position >= _buffer.Length; }
        }

        public bool MoveNext()
        {
            _currentPosition.InputLength = 0;

            while (true)
            {
                if (++_currentPosition.Position >= _buffer.Length)
                {
                    _currentPosition.InputLength = 0;
                    return false;
                }

                var c = _buffer[_currentPosition.Position];
                _currentPosition.InputLength++;

                if (c == '\r' || c == '\n')
                {
                    if (c == '\r')
                    {
                        if ((_buffer.Length > _currentPosition.Position + 1) && _buffer[_currentPosition.Position + 1] == '\n')
                            continue;
                    }
                    c = '\n';

                    if (++_currentPosition.BlankLines > 2) continue;
                }
                else
                {
                    _currentPosition.BlankLines = 0;
                }

                _currentPosition.Character = c;
                return true;
            }
        }

        public char? Peek()
        {
            var currentPosition = _currentPosition.Position;
            var currentCharacter = _currentPosition.Character;

            if (!MoveNext()) return null;

            var c = _currentPosition.Character;

            _currentPosition.Position = currentPosition;
            _currentPosition.Character = currentCharacter;

            return c;
        }

        public void Reset()
        {
            _currentPosition = new CharacterStreamPosition
            {
                Position = -1
            };
        }

        #endregion

        #region ICharacterStreamWriter

        private char _lastCharacter;
        private int _lineLength;
        private bool _blankLine;

        private readonly List<char> _allowedCharacters = new List<char> 
        { 
            '`', '~', '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '-', '_', '+', '=',
            '[', ']', '{', '}', '\\', '|', ';', ':', '"', '\'', '<', '>', ',', 
            '.', '/', '?', ' ', '\n', '\t'
        };

        public void Write(char c)
        {
            if (char.IsLetter(c) || char.IsNumber(c) || _allowedCharacters.Contains(c))
            {
                switch (c)
                {
                    case (' '):
                    case ('\n'):
                    case ('\t'):
                        if (_lastCharacter != ' ' || !BreakLongLines)
                            InternalWrite(' ');
                        break;
                    default:
                        InternalWrite(c);
                        break;
                }
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
            if (s == null) return;

            _writer.Write('"');
            _writer.Write(s.Replace("\"", "'"));
            _writer.Write('"');
        }

        public void WriteBlockComment(string s)
        {
            // Markdown doesn't have a way to include comments
        }

        public void WriteLineBreak()
        {
            if (!_blankLine)
                InternalWrite('\n');
        }

        private void InternalWrite(char c)
        {
            if (c == ' ' && BreakLongLines)
            {
                if (_lineLength == 0)
                    return;

                if (_lineLength > 60)
                    c = '\n';
            }

            if (_lineLength == 0 && State == MarkdownStates.BlockQuote && c != '\n')
                _writer.Write("> ");

            _writer.Write(c);

            _blankLine = c == '\n' && _lastCharacter == '\n';
            _lastCharacter = c;
            _lineLength = c == '\n' ? 0 : _lineLength + 1;
        }

        private bool BreakLongLines
        {
            get { return 
                State == MarkdownStates.Paragraph || 
                State == MarkdownStates.ParagraphBreak ||
                State == MarkdownStates.BlockQuote; }
        }

        #endregion

        private class CharacterStreamPosition : ICharacterStreamPosition
        {
            public int BlankLines;
            public int Position;
            public int InputLength;
            public char Character;
        }
    }
}