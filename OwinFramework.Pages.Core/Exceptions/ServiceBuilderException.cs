using System;

namespace OwinFramework.Pages.Core.Exceptions
{
    /// <summary>
    /// Thrown by service builders when the service definition is not valid
    /// </summary>
    public class ServiceBuilderException: BuilderException
    {
        /// <summary>
        /// Constructs a service builder exception
        /// </summary>
        public ServiceBuilderException() 
            : base() { }

        /// <summary>
        /// Constructs a service builder exception
        /// </summary>
        public ServiceBuilderException(string message) 
            : base(message) { }

        /// <summary>
        /// Constructs a service builder exception
        /// </summary>
        public ServiceBuilderException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
