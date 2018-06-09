using System;

namespace OwinFramework.Pages.Core.Exceptions
{
    /// <summary>
    /// Thrown by page builders when the page definition is not valid
    /// </summary>
    public class PageBuilderException : BuilderException
    {
        /// <summary>
        /// Constructs a page builder exception
        /// </summary>
        public PageBuilderException() 
            : base() { }

        /// <summary>
        /// Constructs a page builder exception
        /// </summary>
        public PageBuilderException(string message) 
            : base(message) { }

        /// <summary>
        /// Constructs a page builder exception
        /// </summary>
        public PageBuilderException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
