using OwinFramework.Pages.Core.Interfaces;
using System;

namespace OwinFramework.Pages.Core.Exceptions
{
    /// <summary>
    /// Thrown by the builders when the buyild is not valid
    /// </summary>
    public class NameResolutionFailureException : NameManagerException
    {
        /// <summary>
        /// Constructs a builder exception
        /// </summary>
        public NameResolutionFailureException(Type elementType, IPackage package, string name) 
            : base("Failed to resolve " + elementType.Name + " '" + name + "' from " + 
                (package == null 
                    ? "global namespace" 
                    : package.Name + " package with namespace '"+package.NamespaceName+"'"))
        { }
    }
}
