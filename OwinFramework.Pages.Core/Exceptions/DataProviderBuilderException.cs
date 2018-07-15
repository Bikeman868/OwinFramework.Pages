using System;

namespace OwinFramework.Pages.Core.Exceptions
{
    /// <summary>
    /// Thrown by component builders when the component definition is not valid
    /// </summary>
    public class DataProviderBuilderException : BuilderException
    {
        /// <summary>
        /// Constructs a component builder exception
        /// </summary>
        public DataProviderBuilderException() 
            : base() { }

        /// <summary>
        /// Constructs a component builder exception
        /// </summary>
        public DataProviderBuilderException(string message) 
            : base(message) { }

        /// <summary>
        /// Constructs a component builder exception
        /// </summary>
        public DataProviderBuilderException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
