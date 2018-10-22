using System.Collections.Generic;

namespace OwinFramework.Pages.Html.Templates.Text
{
    internal interface ICharacterStreamPosition { }

    internal interface ICharacterStreamReader: IEnumerator<char>
    {
        /// <summary>
        /// Capture the current position so we can return to it later
        /// </summary>
        ICharacterStreamPosition CurrentPosition { get; }

        /// <summary>
        /// Return to a previously saved position
        /// </summary>
        /// <param name="position">The position to return to</param>
        void Reset(ICharacterStreamPosition position);

        /// <summary>
        /// The raw input that resulted in the current character. For example in HTML
        /// the imput string '&gt;' translates into a single '>' character in the 
        /// character stream
        /// </summary>
        string CurrentRawInput { get; }

        /// <summary>
        /// Returns true with the end of the document is reached
        /// </summary>
        bool Eof { get; }

        /// <summary>
        /// Peeks ahead at the next character in the input stream
        /// </summary>
        /// <returns></returns>
        char? Peek();
    }
}
