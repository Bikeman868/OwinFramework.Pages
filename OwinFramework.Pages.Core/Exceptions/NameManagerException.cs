using System;

namespace OwinFramework.Pages.Core.Exceptions
{
    /// <summary>
    /// Thrown by the name manager when there are naming 
    /// conflicts or name resolution failures
    /// </summary>
    public class NameManagerException: Exception
    {
        /// <summary>
        /// Constructs a NameManagerException
        /// </summary>
        public NameManagerException() 
            : base() { }

        /// <summary>
        /// Constructs a NameManagerException
        /// </summary>
        public NameManagerException(string message) 
            : base(message) { }

        /// <summary>
        /// Constructs a NameManagerException
        /// </summary>
        public NameManagerException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
