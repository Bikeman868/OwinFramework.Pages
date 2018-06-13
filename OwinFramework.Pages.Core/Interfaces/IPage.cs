using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces
{
    /// <summary>
    /// A page produces html in response to a request from a browser
    /// </summary>
    public interface IPage : IRunable
    {
        /// <summary>
        /// The unique name of this element for its type within the package
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Optional package - not all elements have to be packaged
        /// </summary>
        IPackage Package { get; set; }

        /// <summary>
        /// This method is called after all names have been resolves and all
        /// elements have been wired together. This is where the page has an
        /// oportunity to do work once that makes the rendering of each instance
        /// faster.
        /// </summary>
        void Initialize();
    }
}
