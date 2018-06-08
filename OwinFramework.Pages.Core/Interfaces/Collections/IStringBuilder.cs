using System;
using System.Collections.Generic;

namespace OwinFramework.Pages.Core.Interfaces.Collections
{
    /// <summary>
    /// Defines a disposable/reusable version of StringBuilder that uses the garbage
    /// collector much more efficiently
    /// </summary>
    public interface IStringBuilder : IDisposable, IEnumerable<char>
    {
        /// <summary>
        /// Gets or sets the number of characters in the string builder
        /// </summary>
        long Length { get; set; }

        /// <summary>
        /// Appends text to the buffer
        /// </summary>
        IStringBuilder Append(string text);

        /// <summary>
        /// Appends a character to the buffer
        /// </summary>
        IStringBuilder Append(char text);

        /// <summary>
        /// Appends a formatted string to the buffer
        /// </summary>
        IStringBuilder AppendFormat(string format, params object[] arguments);

        /// <summary>
        /// Appends text and a line break to the buffer
        /// </summary>
        IStringBuilder AppendLine(string line);

        /// <summary>
        /// Appends the string representation of a value to the buffer
        /// </summary>
        /// <typeparam name="T">The type of data to append</typeparam>
        /// <param name="value">The value to append</param>
        IStringBuilder Append<T>(T value);

        /// <summary>
        /// Deletes all of the data in the buffer and sets the length
        /// back to zero
        /// </summary>
        void Clear();

        /// <summary>
        /// Extracts the buffered text into a string
        /// </summary>
        string ToString();

        /// <summary>
        /// Retrieves the buffer from within the string builder
        /// </summary>
        /// <param name="extractBuffer">Pass true to take ownership of this array</param>
        IArray<char> ToArray(bool extractBuffer = false);
    }
}
