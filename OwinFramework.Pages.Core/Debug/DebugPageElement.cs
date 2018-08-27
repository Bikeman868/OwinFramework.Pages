using OwinFramework.Pages.Core.Interfaces;

namespace OwinFramework.Pages.Core.Debug
{
    /// <summary>
    /// Debug information about an element
    /// </summary>
    public class DebugPageElement: DebugInfo
    {
        /// <summary>
        /// Default public constructor
        /// </summary>
        public DebugPageElement()
        {
            Type = "Page element";
        }

        /// <summary>
        /// Returns a default description
        /// </summary>
        public override string ToString()
        {
            return Type + " '" + (Name ?? string.Empty) + "'";
        }

        /// <summary>
        /// The element that defines this page element
        /// </summary>
        public IElement Element;

        /// <summary>
        /// The parent of this element or null if this is the page
        /// </summary>
        public DebugPageElement Parent;

        /// <summary>
        /// The children of this element if any
        /// </summary>
        public DebugPageElement[] Children;
    }
}
