using System;
using System.IO;

namespace OwinFramework.Pages.Core.Interfaces.Collections
{
    /// <summary>
    /// Defines a re-usable memory stream. Re-using memory streams avoids garbage collection
    /// </summary>
    public interface IMemoryStream: IDisposable
    {
        /// <summary>
        /// Gets a Stream instance that can be used with other
        /// </summary>
        /// <returns></returns>
        Stream AsStream();

        /// <summary>
        /// Converts the contents of the in-memory stream to an array of bytes
        /// </summary>
        byte[] ToArray();

        /// <summary>
        /// Writes an array of bytes into the stream
        /// </summary>
        void Write(byte[] buffer, long offset, long count);

        /// <summary>
        /// Writes a single byte into the stream
        /// </summary>
        void WriteByte(byte value);

        /// <summary>
        /// Returns the count of bytes written to the stream
        /// </summary>
        long Length { get; }

        /// <summary>
        /// Gets/sets the position where the next byte will be written
        /// </summary>
        long Position { get; set; }
    }
}
