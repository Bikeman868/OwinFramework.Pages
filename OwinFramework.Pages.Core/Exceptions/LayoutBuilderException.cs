using System;

namespace OwinFramework.Pages.Core.Exceptions
{
    /// <summary>
    /// Thrown by layout builders when the layout definition is not valid
    /// </summary>
    public class LayoutBuilderException : BuilderException
    {
        /// <summary>
        /// Constructs a layout builder exception
        /// </summary>
        public LayoutBuilderException() 
            : base() { }

        /// <summary>
        /// Constructs a layout builder exception
        /// </summary>
        public LayoutBuilderException(string message) 
            : base(message) { }

        /// <summary>
        /// Constructs a layout builder exception
        /// </summary>
        public LayoutBuilderException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
