namespace OwinFramework.Pages.Core.Interfaces.Runtime
{
    /// <summary>
    /// Defines a mechanism for constructing render contexts
    /// </summary>
    public interface IRenderContextFactory
    {
        /// <summary>
        /// Creates a new render context. The render context needs to
        /// be initialized for a request before it can be used.
        /// </summary>
        IRenderContext Create();
    }
}
