using System;

namespace OwinFramework.Pages.Core.Exceptions
{
    /// <summary>
    /// Thrown by template builders when the template definition is not valid
    /// </summary>
    public class TemplateBuilderException : BuilderException
    {
        /// <summary>
        /// Constructs a template builder exception
        /// </summary>
        public TemplateBuilderException() 
            : base() { }

        /// <summary>
        /// Constructs a template builder exception
        /// </summary>
        public TemplateBuilderException(string message) 
            : base(message) { }

        /// <summary>
        /// Constructs a template builder exception
        /// </summary>
        public TemplateBuilderException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
