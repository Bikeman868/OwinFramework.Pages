using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Interfaces.Collections;

namespace OwinFramework.Pages.Core.Interfaces.Runtime
{
    /// <summary>
    /// A TextWriter that writes HTML to the response. Has special features for
    /// writing HTML elements and also for allowing multiple threads to simultaneously
    /// write into different parts of the output buffer.
    /// </summary>
    public interface ICssWriter: IDisposable
    {
        /// <summary>
        /// Turn indentation off to reduce the size of the html
        /// </summary>
        bool Indented { get; set; }

        /// <summary>
        /// Turn comments off to reduce the size of the html
        /// </summary>
        bool IncludeComments { get; set; }

        /// <summary>
        /// Writes a CSS style rule into the buffered CSS document
        /// </summary>
        /// <param name="selector">The CSS selector for this style rule. For example 'h1.{ns}_content'</param>
        /// <param name="styles">Semicolon separated list of styles to apply. For example 'font-size: 12px; font-weight: bold;'</param>
        /// <param name="package">Optional package for namespace replacement</param>
        ICssWriter WriteRule(string selector, string styles, IPackage package = null);

        /// <summary>
        /// Writes a comment into the buffered CSS
        /// </summary>
        ICssWriter WriteComment(string comment);

        /// <summary>
        /// Provides a mechanism to write preformatted css to the css file
        /// </summary>
        ICssWriter WriteLineRaw(string line);

        /// <summary>
        /// Allows you to check if any css styles were written
        /// </summary>
        bool HasContent { get; }

        /// <summary>
        /// Writes the buffered CSS into an Html document
        /// </summary>
        void ToHtml(IHtmlWriter html);

        /// <summary>
        /// Writes the buffered CSS to a string builder
        /// </summary>
        void ToStringBuilder(IStringBuilder stringBuilder);

        /// <summary>
        /// Writes the buffered CSS to a list of lines
        /// </summary>
        IList<string> ToLines();
    }
}
