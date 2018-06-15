namespace OwinFramework.Pages.Core.Enums
{
    /// <summary>
    /// Used to fully qualify element names. Combining the package name,
    /// element type and element name produces a enique name for an
    /// element
    /// </summary>
    public enum ElementType 
    { 
        /// <summary>
        /// This element is of some other type where the fully
        /// qualified name is not required
        /// </summary>
        Unnamed = 0,

        /// <summary>
        /// The element is a page
        /// </summary>
        Page,

        /// <summary>
        /// The element is a layout
        /// </summary>
        Layout,

        /// <summary>
        /// The element is a region
        /// </summary>
        Region,

        /// <summary>
        /// The element is a component
        /// </summary>
        Component
    }
}
