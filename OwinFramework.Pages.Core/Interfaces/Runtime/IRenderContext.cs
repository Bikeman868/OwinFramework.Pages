using Microsoft.Owin;

namespace OwinFramework.Pages.Core.Interfaces.Runtime
{
    /// <summary>
    /// An instance of this interface is associated with each request and has
    /// the same lifetime as the request processing
    /// </summary>
    public interface IRenderContext
    {
        /// <summary>
        /// Returns the Owin Context. This provides access to the request.
        /// You can set response headers etc, but you must not use this to
        /// write to the response stream.
        /// </summary>
        IOwinContext OwinContext { get; }

        /// <summary>
        /// Returns an instance of IHtmlWriter that can be used to write
        /// html to the response stream
        /// </summary>
        IHtmlWriter Html { get; }

        /// <summary>
        /// Returns the language that this page should be rendered in
        /// </summary>
        string Language { get; }

        /// <summary>
        /// Initializes the render context for a specific request
        /// </summary>
        IRenderContext Initialize(IOwinContext context);
    }
}
