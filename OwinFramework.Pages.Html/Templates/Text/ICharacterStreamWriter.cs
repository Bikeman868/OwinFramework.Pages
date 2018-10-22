namespace OwinFramework.Pages.Html.Templates.Text
{
    internal interface ICharacterStreamWriter
    {
        /// <summary>
        /// Writes a character to the output stream. Encodes special characters
        /// where necessary
        /// </summary>
        void Write(char c);

        /// <summary>
        /// Writes a string of characters to the output stream encoding special
        /// characters as necessary
        /// </summary>
        void Write(string s);

        /// <summary>
        /// Writes a string enclosed in quotation marks. The character to use
        /// for the quote marks is implementation specific. Different languages/formats
        /// have different rules for enclosing text in quotes and escaping quote marks
        /// inside the string. This method encapsulates those rules.
        /// </summary>
        void WriteQuotedString(string s);

        /// <summary>
        /// Writes a comment. Different languages/formats have different rules for 
        /// including comments. This method encapsulates those rules.
        /// </summary>
        void WriteBlockComment(string s);

        /// <summary>
        /// Writes a line break character specific to this format. Could be \r or \n or \r\n etc
        /// </summary>
        void WriteLineBreak();
    }
}
