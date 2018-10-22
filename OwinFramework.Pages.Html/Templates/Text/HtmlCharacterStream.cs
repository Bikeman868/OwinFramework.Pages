using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Linq;

namespace OwinFramework.Pages.Html.Templates.Text
{
    // https://www.w3.org/TR/html/syntax.html#parsing-html-documents

    internal class HtmlCharacterStream: ICharacterStreamReader, ICharacterStreamWriter
    {
        private readonly string _buffer;
        private readonly System.IO.TextWriter _writer;

        private Dictionary<string, char> _entityCodes;
        private Dictionary<char, string> _entityNames;

        private int _currentPosition;
        private int _currentInputLength;
        private char _currentCharacter;

        public HtmlStates State = HtmlStates.Data;

        public HtmlCharacterStream(TextReader reader)
        {
            _buffer = reader.ReadToEnd();
            LoadHtmlEntities();
            Reset();
        }

        public HtmlCharacterStream(System.IO.TextWriter writer)
        {
            _writer = writer;
            LoadHtmlEntities();
        }

        void IDisposable.Dispose()
        {
        }

        #region Entity lookup tables

        private void LoadHtmlEntities()
        {
            var entityCodes = new Dictionary<string, char>();
            var entityNames = new Dictionary<char, string>();

            var scriptResourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames().FirstOrDefault(n => n.Contains("html-entities.json"));
            if (scriptResourceName == null)
                throw new Exception("Html entities are missing from embedded resources");

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(scriptResourceName))
            {
                if (stream == null)
                    throw new Exception("There was a problem reading Html entities from the embedded resource");
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    var jsonText = reader.ReadToEnd();
                    var json = JObject.Parse(jsonText);
                    foreach (var entity in json.Properties())
                    {
                        // NOTE: This does not work for  surrogate pairs
                        var entityValue = entity.Value as JObject;
                        var codePoints = entityValue["codepoints"] as JArray;
                        var firstCodePoint = codePoints[0].Value<int>();
                        var c = char.ConvertFromUtf32(firstCodePoint)[0];
                        entityCodes[entity.Name] = c;
                        if (codePoints.Count == 1 && !entityNames.ContainsKey(c))
                        {
                            entityNames[c] = entity.Name;
                        }
                    }
                }
            }

            _entityCodes = entityCodes;
            _entityNames = entityNames;
        }

        #endregion

        #region ICharacterStreamReader

        public ICharacterStreamPosition CurrentPosition
        {
            get 
            { 
                return new CharacterStreamPosition 
                { 
                    Position = _currentPosition,
                    InputLength = _currentInputLength,
                    Character = _currentCharacter
                }; 
            }
        }

        public void Reset(ICharacterStreamPosition position)
        {
            var p = (CharacterStreamPosition)position;
            _currentPosition = p.Position;
            _currentInputLength = p.InputLength;
            _currentCharacter = p.Character;
        }

        public char Current
        {
            get { return _currentCharacter; }
        }

        object System.Collections.IEnumerator.Current
        {
            get { return _currentCharacter; }
        }

        public string CurrentRawInput 
        { 
            get
            {
                return _buffer.Substring(_currentPosition - _currentInputLength + 1, _currentInputLength);
            }
        }

        public bool Eof
        {
            get { return _currentPosition >= _buffer.Length; }
        }

        public bool MoveNext()
        {
            while (true)
            {
                if (++_currentPosition >= _buffer.Length)
                {
                    _currentInputLength = 0;
                    return false;
                }

                var c = _buffer[_currentPosition];
                _currentInputLength = 1;

                if (c == '\r' || c == '\n')
                {
                    if (_currentPosition > 0 && (_buffer[_currentPosition - 1] == '\n' || _buffer[_currentPosition - 1] == '\r'))
                        continue;
                    c = '\n';
                }

                if (c == '&' && _characterReferenceStates.Contains(State))
                {
                    _currentCharacter = c;

                    var maximumcharacterReferenceLength = 10;
                    if (maximumcharacterReferenceLength > _buffer.Length - _currentPosition - 1)
                        maximumcharacterReferenceLength = _buffer.Length - _currentPosition - 1;

                    var semicolonPos = _buffer.IndexOf(';', _currentPosition + 1, maximumcharacterReferenceLength);
                    if (semicolonPos < _currentPosition) return true; // Not a valid character reference, return ampersand

                    var characterReference = _buffer.Substring(_currentPosition, semicolonPos - _currentPosition + 1);
                    if (characterReference.Length < 3) return true; // Not a valid character reference, return ampersand

                    if (characterReference[1] == '#')
                    {
                        // Character reference is a numeric code

                        if (characterReference.Length < 4) return true; // Not a valid character reference, return ampersand

                        int characterCode;
                        if (characterReference[2] == 'x' || characterReference[2] == 'X')
                        {
                            try
                            {
                                characterCode = int.Parse(characterReference.Substring(3, characterReference.Length - 4), System.Globalization.NumberStyles.HexNumber);
                                c = char.ConvertFromUtf32(characterCode)[0];
                                _currentInputLength = characterReference.Length;
                                _currentPosition = semicolonPos;
                            }
                            catch
                            {
                                return true; // Not a valid character reference, return ampersand
                            }
                        }
                        if (int.TryParse(characterReference.Substring(2, characterReference.Length - 3), out characterCode))
                        {
                            c = char.ConvertFromUtf32(characterCode)[0];
                            _currentInputLength = characterReference.Length;
                            _currentPosition = semicolonPos;
                        }
                    }
                    else
                    {
                        // Character reference is a named entity

                        lock (_entityCodes)
                        {
                            char characterCode;
                            if (_entityCodes.TryGetValue(characterReference, out characterCode))
                            {
                                c = characterCode;
                                _currentInputLength = characterReference.Length;
                                _currentPosition = semicolonPos;
                            }
                        }
                    }
                }

                _currentCharacter = c;
                return true;
            }
        }

        public char? Peek()
        {
            var currentPosition = _currentPosition;
            var currentCharacter = _currentCharacter;

            if (!MoveNext()) return null;

            var c = _currentCharacter;

            _currentPosition = currentPosition;
            _currentCharacter = currentCharacter;

            return c;
        }

        public void Reset()
        {
            _currentPosition = -1;
        }

        #endregion

        #region ICharacterStreamWriter

        private readonly List<char> _allowedCharacters = new List<char> 
        { 
            ',', '.', ';', '\'', '"', '@', '#', '$', '%', '^', '*', '(', ')', '-', '=', '_', '+',
            '{', '}', '[', ']', ':', '?', '/', '\\', '|', '>'
        };

        public void Write(char c)
        {
            if (c == '\r')
            {
            }
            else if (char.IsLetter(c) || char.IsNumber(c) || char.IsWhiteSpace(c) || _allowedCharacters.Contains(c))
            {
                _writer.Write(c);
            }
            else if (_characterReferenceStates.Contains(State))
            {
                string entityName;
                if (_entityNames.TryGetValue(c, out entityName))
                {
                    _writer.Write(entityName);
                }
                else
                {
                    _writer.Write("&#");
                    _writer.Write((int)c);
                    _writer.Write(';');
                }
            }
            else
            {
                _writer.Write(c);
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

            if (s.IndexOf('\'') >= 0)
            {
                _writer.Write('"');
                _writer.Write(s.Replace("\"", "&quot;"));
                _writer.Write('"');
            }
            else
            {
                _writer.Write('\'');
                _writer.Write(s);
                _writer.Write('\'');
            }
        }

        public void WriteBlockComment(string s)
        {
            _writer.Write("<!-- ");
            _writer.Write(s);
            _writer.Write(" -->");
        }

        public void WriteLineBreak()
        {
            _writer.Write('\n');
        }

        #endregion

        #region States

        private readonly HtmlStates[] _textStates = 
        { 
            HtmlStates.Data, 
            HtmlStates.RawText, 
            HtmlStates.PlainText, 
            HtmlStates.AttributeValueDoubleQuoted, 
            HtmlStates.AttributeValueSingleQuoted
        };

        private readonly HtmlStates[] _characterReferenceStates = 
        { 
            HtmlStates.Data, 
            HtmlStates.RcData, 
            HtmlStates.BeforeAttributeValue, 
            HtmlStates.AttributeValueSingleQuoted, 
            HtmlStates.AttributeValueDoubleQuoted,
            HtmlStates.AttributeValueUnquoted
        };

        private readonly HtmlStates[] _singleQuoteStates = 
        { 
            HtmlStates.AttributeValueSingleQuoted, 
            HtmlStates.DocTypePublicIdentifierSingleQuoted, 
            HtmlStates.DocTypeSystemIdentifierSingleQuoted
        };

        private readonly HtmlStates[] _doubleQuoteStates = 
        { 
            HtmlStates.AttributeValueDoubleQuoted, 
            HtmlStates.DocTypePublicIdentifierDoubleQuoted, 
            HtmlStates.DocTypeSystemIdentifierDoubleQuoted
        };

        private class CharacterStreamPosition: ICharacterStreamPosition
        {
            public int Position;
            public int InputLength;
            public char Character;
        }

        #endregion
    }
}