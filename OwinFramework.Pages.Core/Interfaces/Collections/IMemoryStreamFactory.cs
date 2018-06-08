using System;

namespace OwinFramework.Pages.Core.Interfaces.Collections
{
    /// <summary>
    /// Defines the ability to return instances of IMemoryStream
    /// </summary>
    public interface IMemoryStreamFactory
    {
        /// <summary>
        /// Returns an empty memory stream
        /// </summary>
        IMemoryStream Create();

        /// <summary>
        /// Creates an initializes a memory stream
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        IMemoryStream Create(Byte[] data);
    }
}
