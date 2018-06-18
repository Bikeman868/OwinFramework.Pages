using System;

namespace OwinFramework.Pages.Core.Exceptions
{
    /// <summary>
    /// Thrown by the fluent builders when the element definition is not valid
    /// </summary>
    public class FluentBuilderException : BuilderException
    {
        /// <summary>
        /// Constructs a fluent builder exception
        /// </summary>
        public FluentBuilderException() 
            : base() { }

        /// <summary>
        /// Constructs a fluent builder exception
        /// </summary>
        public FluentBuilderException(string message) 
            : base(message) { }

        /// <summary>
        /// Constructs a fluent builder exception
        /// </summary>
        public FluentBuilderException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
