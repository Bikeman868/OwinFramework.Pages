using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces
{
    /// <summary>
    /// A page produces html in response to a request from a browser
    /// </summary>
    public interface IPage : IRunable, INamed, IPackagable
    {
        /// <summary>
        /// Gets debugging information from this page
        /// </summary>
        new DebugPage GetDebugInfo();

        /// <summary>
        /// This method is called after all names have been resolves and all
        /// elements have been wired together. This is where the page has an
        /// oportunity to do work once that makes the rendering of each instance
        /// faster.
        /// </summary>
        void Initialize();
    }
}
