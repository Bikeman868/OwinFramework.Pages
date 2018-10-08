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
            : base(
                "Element names can only contain numbers, letters and underscore" +
                " because they are used as a base for JavaScript function and css" +
                " class names. Invalid name '" + name + "' in " + 
                (package == null 
                    ? "global namespace" 
                    : package.Name + " package with namespace '" + package.NamespaceName + "'"))
        { }
    }
}
