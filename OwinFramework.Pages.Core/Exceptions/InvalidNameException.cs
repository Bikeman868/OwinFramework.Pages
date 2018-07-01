using OwinFramework.Pages.Core.Interfaces;

namespace OwinFramework.Pages.Core.Exceptions
{
    /// <summary>
    /// Thrown by the builders when the buyild is not valid
    /// </summary>
    public class InvalidNameException : NameManagerException
    {
        /// <summary>
        /// Constructs a builder exception
        /// </summary>
        public InvalidNameException(string name, IPackage package) 
            : base("Invalid name specified '" + name + "' in " + 
                (package == null 
                    ? "global namespace" 
                    : package.Name + " package with namespace '" + package.NamespaceName+"'"))
        { }
    }
}
