using System;

namespace OwinFramework.Pages.Core.Exceptions
{
    /// <summary>
    /// Thrown by package builders when the package definition is not valid
    /// </summary>
    public class PackageBuilderException : BuilderException
    {
        /// <summary>
        /// Constructs a package builder exception
        /// </summary>
        public PackageBuilderException() 
            : base() { }

        /// <summary>
        /// Constructs a package builder exception
        /// </summary>
        public PackageBuilderException(string message) 
            : base(message) { }

        /// <summary>
        /// Constructs a package builder exception
        /// </summary>
        public PackageBuilderException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
