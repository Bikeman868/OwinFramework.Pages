using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Enums;

namespace OwinFramework.Pages.Core.Interfaces
{
    /// <summary>
    /// A module is a collection of pages that share the same CSS and JavaScript files
    /// </summary>
    public interface IModule
    {
        /// <summary>
        /// The unique name of this module
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets debugging information from this element
        /// </summary>
        DebugModule GetDebugInfo();

        /// <summary>
        /// Defines how the elements in this module have theor assets deployed
        /// </summary>
        AssetDeployment AssetDeployment { get; set; }
    }
}
