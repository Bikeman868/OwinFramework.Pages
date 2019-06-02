using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Collections;

namespace OwinFramework.Pages.Core.Interfaces.Runtime
{
    /// <summary>
    /// A TextWriter that writes HTML to the response. Has special features for
    /// writing HTML elements and also for allowing multiple threads to simultaneously
    /// write into different parts of the output buffer.
    /// </summary>
    public interface IJavascriptWriter: IDisposable
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
        /// Specifies how much to indent new lines. In pretty mode
        /// writes a number of spaces for each level of indent at the
        /// start of every line.
        /// </summary>
        int IndentLevel { get; set; }

        /// <summary>
        /// Returns true is some Javascript was written
        /// </summary>
        bool HasContent { get; }

        /// <summary>
        /// Writes a variable declaration and initialization
        /// </summary>
        /// <param name="variableName">The name of the variable to define</param>
        /// <param name="initializationExpression">A Javascript expression that initializes the variable or null for no initialization</param>
        /// <param name="type">The data type of the variable or null for untyped</param>
        /// <param name="package">The package that this Javascript is part of</param>
        /// <param name="isPublic">Pass true to make this variable visible outside of iss namespace</param>
        IJavascriptWriter WriteVariable(string variableName, string initializationExpression = null, string type = null, IPackage package = null, bool isPublic = false);

        /// <summary>
        /// Writes a Javascript function into the buffer
        /// </summary>
        /// <param name="functionName">The name of the function to write</param>
        /// <param name="parameters">Function parameters - do not include the round braces. For example 'int count'</param>
        /// <param name="functionBody">The body of the function - do not include the curly braces. For example 'alert('hello');'</param>
        /// <param name="returnType">The data type of the return value of null for untyped</param>
        /// <param name="package">The package to write this funtion into</param>
        /// <param name="isPublic">Pass false to make this function private to the package</param>
        IJavascriptWriter WriteFunction(string functionName, string parameters, string functionBody, string returnType, IPackage package, bool isPublic = true);

        /// <summary>
        /// Writes a Javascript class into the buffer
        /// </summary>
        /// <param name="className">The name of the class to write</param>
        /// <param name="classBody">The body of the class definition. Should return an object containing public properties and methods of the class</param>
        /// <param name="package">The package to write this class into</param>
        /// <param name="isPublic">Pass false to make this class private to the package</param>
        IJavascriptWriter WriteClass(string className, string classBody, IPackage package, bool isPublic = true);

        /// <summary>
        /// Writes a comment into the Javascript
        /// </summary>
        IJavascriptWriter WriteComment(string comment, CommentStyle commentStyle = CommentStyle.SingleLineC, IPackage package = null);

        /// <summary>
        /// Provides a mechanism for writing pre-formatted JavaScript to the JavaScript file
        /// </summary>
        IJavascriptWriter WriteLineRaw(string line, IPackage package = null);

        /// <summary>
        /// Writes the buffered JavaScript into an Html document
        /// </summary>
        void ToHtml(IHtmlWriter html);

        /// <summary>
        /// Writes the buffered JavaScript to a string builder
        /// </summary>
        void ToStringBuilder(IStringBuilder stringBuilder);

        /// <summary>
        /// Writes the buffered JavaScript to a list of lines
        /// </summary>
        IList<string> ToLines();
    }
}
