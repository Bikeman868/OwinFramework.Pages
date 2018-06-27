using OwinFramework.Pages.Core.Debug;

namespace OwinFramework.Pages.Core.Interfaces
{
    /// <summary>
    /// Components render html, css and JavaScript into the output
    /// </summary>
    public interface IComponent : IElement
    {
        /// <summary>
        /// Retrieves debugging information from this component
        /// </summary>
        new DebugComponent GetDebugInfo();
    }
}
