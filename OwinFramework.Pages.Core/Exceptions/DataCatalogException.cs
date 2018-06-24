using System;

namespace OwinFramework.Pages.Core.Exceptions
{
    /// <summary>
    /// Thrown by the data catalog when requested data is not available
    /// </summary>
    public class DataCatalogException: Exception
    {
        /// <summary>
        /// Constructs a data catalog exception
        /// </summary>
        public DataCatalogException() 
            : base() { }

        /// <summary>
        /// Constructs a data catalog exception
        /// </summary>
        public DataCatalogException(string message) 
            : base(message) { }

        /// <summary>
        /// Constructs a data catalog exception
        /// </summary>
        public DataCatalogException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
