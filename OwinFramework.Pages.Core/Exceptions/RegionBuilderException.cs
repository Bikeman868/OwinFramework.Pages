using System;

namespace OwinFramework.Pages.Core.Exceptions
{
    /// <summary>
    /// Thrown by region builders when the region definition is not valid
    /// </summary>
    public class RegionBuilderException : BuilderException
    {
        /// <summary>
        /// Constructs a region builder exception
        /// </summary>
        public RegionBuilderException() 
            : base() { }

        /// <summary>
        /// Constructs a region builder exception
        /// </summary>
        public RegionBuilderException(string message) 
            : base(message) { }

        /// <summary>
        /// Constructs a region builder exception
        /// </summary>
        public RegionBuilderException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
