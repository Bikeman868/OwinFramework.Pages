using System;

namespace OwinFramework.Pages.Core.Exceptions
{
    /// <summary>
    /// Thrown by the builders when the buyild is not valid
    /// </summary>
    public class PageBuilderException: Exception
    {
        /// <summary>
        /// Constructs a builder exception
        /// </summary>
        public PageBuilderException() 
            : base() { }

        /// <summary>
        /// Constructs a builder exception
        /// </summary>
        public PageBuilderException(string message) 
            : base(message) { }

        /// <summary>
        /// Constructs a builder exception
        /// </summary>
        public PageBuilderException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
