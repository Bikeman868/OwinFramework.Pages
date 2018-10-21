using System;

namespace OwinFramework.Pages.Core.Exceptions
{
    /// <summary>
    /// Thrown by template loaders when there was a problem loading templates
    /// </summary>
    public class TemplateLoaderException : TemplateBuilderException
    {
        /// <summary>
        /// Constructs a template loader exception
        /// </summary>
        public TemplateLoaderException() 
            : base() { }

        /// <summary>
        /// Constructs a template loader exception
        /// </summary>
        public TemplateLoaderException(string message) 
            : base(message) { }

        /// <summary>
        /// Constructs a template loader exception
        /// </summary>
        public TemplateLoaderException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
