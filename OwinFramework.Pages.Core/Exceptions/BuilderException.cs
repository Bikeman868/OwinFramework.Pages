using System;

namespace OwinFramework.Pages.Core.Exceptions
{
    /// <summary>
    /// Thrown by the builders when the buyild is not valid
    /// </summary>
    public class BuilderException: Exception
    {
        /// <summary>
        /// Constructs a builder exception
        /// </summary>
        public BuilderException() 
            : base() { }

        /// <summary>
        /// Constructs a builder exception
        /// </summary>
        public BuilderException(string message) 
            : base(message) { }

        /// <summary>
        /// Constructs a builder exception
        /// </summary>
        public BuilderException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
