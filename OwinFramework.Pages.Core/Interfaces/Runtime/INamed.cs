using OwinFramework.Pages.Core.Enums;

namespace OwinFramework.Pages.Core.Interfaces.Runtime
{
    /// <summary>
    /// Implemented by elements that have a name, can be registered and
    /// referenced in other elements by their name
    /// </summary>
    public interface INamed
    {
        /// <summary>
        /// Returns the type of element. This is used to make the fully qualified
        /// element name unique
        /// </summary>
        ElementType ElementType { get; }

        /// <summary>
        /// Tha name must be unique for this type of element within the
        /// same package
        /// </summary>
        string Name { get; set; }
    }
}
