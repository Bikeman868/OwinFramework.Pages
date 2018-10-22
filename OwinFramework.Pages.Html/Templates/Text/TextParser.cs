using System;
using System.Linq;
using OwinFramework.Pages.Core.Interfaces.Collections;

namespace OwinFramework.Pages.Html.Templates.Text
{
    internal class TextParser
    {
        private readonly IStringBuilderFactory _stringBuilderFactory;
        private readonly ICharacterStreamReader _characterStream;

        public TextParser(
            IStringBuilderFactory stringBuilderFactory,
            ICharacterStreamReader characterStream)
        {
            _stringBuilderFactory = stringBuilderFactory;
            _characterStream = characterStream;
        }

        /// <summary>
        /// Takes one character from the input stream
        /// </summary>
        /// <returns></returns>
        public char? TakeOne()
        {
            if (_characterStream.MoveNext())
                return _characterStream.Current;

            return null;
        }

        /// <summary>
        /// Looks at what the next character is going to be
        /// </summary>
        public char? Peek()
        {
            return _characterStream.Peek();
        }

        /// <summary>
        /// Takes an exact count of characters from the input stream
        /// </summary>
        /// <param name="count">The number of characters to take</param>
        /// <param name="sb">The buffer to append the characters to</param>
        /// <param name="includeCurrent">Pass true to include the current character in
        /// the output buffer. Pass false to start taking characters from the next
        /// character in the input stream</param>
        /// <returns>True if there were enough characters in the input stream</returns>
        public bool Take(IStringBuilder sb, int count, bool includeCurrent = true)
        {
            if (count == 0) return true;

            if (includeCurrent)
            {
                sb.Append(_characterStream.Current);
                count--;
            }

            while (count-- > 0)
            {
                var c = TakeOne();
                if (!c.HasValue) return false;
                sb.Append(c.Value);
            }

            return true;
        }

        /// <summary>
        /// Copies characters into an output buffer until a terminator or Eof is reached. 
        /// The terminator is not copied to the output buffer.
        /// </summary>
        /// <param name="isTerminator">A function that tests if the character is a terminator</param>
        /// <param name="maxLength">The maximum length string to extract or 0 for no limit</param>
        /// <param name="sb">The buffer to append the extracted string to</param>
        /// <param name="terminator">Returns the character that terminated the extracted string or null if the end of the input stream was reached</param>
        /// <param name="includeCurrent">Pass true to include the character at the current cursor position</param>
        /// <returns>True if any of the terminators were found within the specified maximum length or the end of the input stream was reached.
        /// When false is returned the input stream is not advanced and nothing is added to the output buffer</returns>
        public bool TakeUntil(IStringBuilder sb, int maxLength, Func<char, bool> isTerminator, out char? terminator, bool includeCurrent = true)
        {
            if (_characterStream.Eof)
            {
                terminator = null;
                return false;
            }

            var start = _characterStream.CurrentPosition;

            using (var result = _stringBuilderFactory.Create(maxLength))
            {
                do
                {
                    char? c;
                    if (includeCurrent)
                    {
                        c = _characterStream.Current;
                        includeCurrent = false;
                    }
                    else
                    {
                        c = TakeOne();
                    }

                    terminator = c;

                    if (c.HasValue)
                    {
                        if (isTerminator(c.Value))
                        {
                            sb.Append(result.ToString());
                            return true;
                        }
                        result.Append(c.Value);
                    }
                    else
                    {
                        sb.Append(result.ToString());
                        return true;
                    }
                } while (maxLength == 0 || result.Length < maxLength);
            }

            _characterStream.Reset(start);
            return false;
        }

        /// <summary>
        /// Skips over characters in the input stream until the specified termination
        /// condition is reached
        /// </summary>
        /// <param name="maxLength">If a terminator is not found within this many characters
        /// then rewind the input stream and return false. Pass 0 for no limit on how many
        /// characters to skip.</param>
        /// <param name="isTerminator">The condition for stopping the skip</param>
        /// <param name="terminator">The character that matched the termination condition.
        /// This will also be the current character in the input stream. Returns will null
        /// when the end of the input stream was reached</param>
        /// <param name="skipTerminator">Set to true to skip over the terminator in the input
        /// stream, set to false if you want the terminator to be the next character to be
        /// read from the input stream</param>
        /// <returns>True if terminator was found</returns>
        public bool SkipUntil(int maxLength, Func<char, bool> isTerminator, out char? terminator, bool skipTerminator = true)
        {
            if (_characterStream.Eof)
            {
                terminator = null;
                return false;
            }

            var start = _characterStream.CurrentPosition;

            do
            {
                terminator = skipTerminator ? TakeOne() : _characterStream.Peek();

                if (!terminator.HasValue) return true;
                if (isTerminator(terminator.Value)) return true;

                if (!skipTerminator)
                {
                    if (!_characterStream.MoveNext())
                        return true;
                }
            } while (maxLength == 0 || --maxLength != 0);

            _characterStream.Reset(start);
            return false;
        }

        /// <summary>
        /// Adds characters up to supplied buffer until the specified terminator is found or there are no 
        /// more characters in the input stream. The terminator is removed from the input stream but not 
        /// appended to the output buffer.
        /// </summary>
        /// <param name="terminator">The terminator character to look for</param>
        /// <param name="maxLength">The maximum length string to extract or 0 for no limit</param>
        /// <param name="sb">The buffer to append the extracted string to</param>
        /// <param name="includeCurrent">Pass true to include the character at the current cursor position</param>
        /// <returns>True if the terminator was found within the specified maximum length.
        /// When false is returned the input stream is not advanced and nothing is
        /// added to the output buffer</returns>
        public bool TakeUntil(IStringBuilder sb, int maxLength, char terminator, bool includeCurrent = true)
        {
            char? c;
            return TakeUntil(sb, maxLength, terminator.Equals, out c, includeCurrent);
        }

        /// <summary>
        /// Adds characters up to supplied buffer until any of specified terminators is found or there are no 
        /// more characters in the input stream. The terminator is removed from the input stream but not 
        /// appended to the output buffer.
        /// </summary>
        /// <param name="terminators">The set of terminator characters to look for</param>
        /// <param name="maxLength">The maximum length string to extract or 0 for no limit</param>
        /// <param name="sb">The buffer to append the extracted string to</param>
        /// <param name="includeCurrent">Pass true to include the character at the current cursor position</param>
        /// <returns>True if any of the terminators were found within the specified maximum length 
        /// of the end of the input stream was reached. When false is returned the input stream is not 
        /// advanced and nothing is added to the output buffer</returns>
        public bool TakeUntilAny(IStringBuilder sb, int maxLength, bool includeCurrent = true, params char[] terminators)
        {
            char? terminator;
            return TakeUntil(sb, maxLength, terminators.Contains, out terminator, includeCurrent);
        }

        /// <summary>
        /// Takes characters up to but not including the next whitespace character. The 
        /// input stream is left positioned at the first whitespace character.
        /// </summary>
        /// <param name="maxLength">The maximum length string to extract or 0 for no limit</param>
        /// <param name="sb">The buffer to append the extracted string to</param>
        /// <param name="includeCurrent">Pass true to include the character at the current cursor position</param>
        /// <returns>True if whitespace was found within the specified maximum length.
        /// When false is returned the input stream is not advanced and nothing is
        /// added to the output buffer</returns>
        public bool TakeUntilWhitespace(IStringBuilder sb, int maxLength, bool includeCurrent = true)
        {
            char? whitespace;
            return TakeUntil(sb, maxLength, char.IsWhiteSpace, out whitespace, includeCurrent);
        }

        /// <summary>
        /// Skips over characters in the input stream
        /// </summary>
        /// <param name="count">The number of characters to skip</param>
        public void Skip(int count)
        {
            while (count-- > 0) TakeOne();
        }

        /// <summary>
        /// Skips over any whitespace leaving the current position at the first
        /// non-whitespace character.
        /// </summary>
        public void SkipWhitespace()
        {
            while (char.IsWhiteSpace(_characterStream.Current) && _characterStream.MoveNext());
        }

        /// <summary>
        /// Skips over any additional whitespace leaving the input stream positioned
        /// at the last whitespace character
        /// </summary>
        public void SkipToLastWhitespace()
        {
            if (!char.IsWhiteSpace(_characterStream.Current))
                return;

            var c = _characterStream.Peek();
            while (c.HasValue && char.IsWhiteSpace(c.Value))
            {
                _characterStream.MoveNext();
                c = _characterStream.Peek();
            }
        }
    }
}