using System;

namespace OwinFramework.Pages.Core.Exceptions
{
    /// <summary>
    /// Thrown by component builders when the component definition is not valid
    /// </summary>
    public class ComponentBuilderException : BuilderException
    {
        /// <summary>
        /// Constructs a component builder exception
        /// </summary>
        public ComponentBuilderException() 
            : base() { }

        /// <summary>
        /// Constructs a component builder exception
        /// </summary>
        public ComponentBuilderException(string message) 
            : base(message) { }

        /// <summary>
        /// Constructs a component builder exception
        /// </summary>
        public ComponentBuilderException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
