using System;

namespace OwinFramework.Pages.Core.Exceptions
{
    /// <summary>
    /// Thrown by module builders when the module definition is not valid
    /// </summary>
    public class ModuleBuilderException : BuilderException
    {
        /// <summary>
        /// Constructs a module builder exception
        /// </summary>
        public ModuleBuilderException() 
            : base() { }

        /// <summary>
        /// Constructs a module builder exception
        /// </summary>
        public ModuleBuilderException(string message) 
            : base(message) { }

        /// <summary>
        /// Constructs a module builder exception
        /// </summary>
        public ModuleBuilderException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
