using OwinFramework.Pages.Core.Interfaces;

namespace OwinFramework.Pages.Core.Exceptions
{
    /// <summary>
    /// Thrown by the builders when the buyild is not valid
    /// </summary>
    public class InvalidPathException : NameManagerException
    {
        /// <summary>
        /// Constructs a builder exception
        /// </summary>
        public InvalidPathException(string path) 
            : base(
                "Template paths can only contain numbers, letters and underscore" +
                " and path separators. Invalid path '" + path + "'")
        { }
    }
}
