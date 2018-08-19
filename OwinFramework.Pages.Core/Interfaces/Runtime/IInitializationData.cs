using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.DataModel;

namespace OwinFramework.Pages.Core.Interfaces.Runtime
{
    /// <summary>
    /// An instance of this interface is passed to the Initialize routine
    /// It is used to gather up all the informatrion needed to get the page ready
    /// </summary>
    public interface IInitializationData
    {
        /// <summary>
        /// Saves the current state on a stack so that changes can be made
        /// and the current state can be restored later
        /// </summary>
        void Push();

        /// <summary>
        /// Restores the previous state from the top of the stack
        /// </summary>
        void Pop();

        /// <summary>
        /// The current asset deployment method to apply to elements
        /// that are set to inherit from their parent. This can be
        /// modified before initializing the children.
        /// </summary>
        AssetDeployment AssetDeployment { get; set; }

        /// <summary>
        /// The scope provider for the current scope
        /// </summary>
        IDataScopeProvider ScopeProvider { get; set; }

        /// <summary>
        /// Introduces a new data scope. All data binding below this point
        /// will resolve using the new scope before deferring to its parent
        /// </summary>
        /// <param name="scopeProvider"></param>
        void AddScope(IDataScopeProvider scopeProvider);

        /// <summary>
        /// Call this method to indicate that this element has a dependancy on
        /// another component that must be rendered onto the page
        /// </summary>
        IInitializationData NeedsComponent(IComponent component);

        /// <summary>
        /// Tells the page about an element instance on this page.
        /// The page uses this information to construct static assets
        /// for the page.
        /// </summary>
        /// <param name="element">An element that exists on this page</param>
        /// <param name="assetDeployment">Defines how this elements assets are configured for deployment</param>
        /// <param name="module">The module that this element is deployed to</param>
        void HasElement(
            IElement element, 
            AssetDeployment assetDeployment = AssetDeployment.Inherit, 
            IModule module = null);

        /// <summary>
        /// Logs an initialization message that can be used to track down
        /// configuration issues.
        /// </summary>
        void Log(string message);
    }
}
