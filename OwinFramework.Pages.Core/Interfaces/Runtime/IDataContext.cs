using Microsoft.Owin;

namespace OwinFramework.Pages.Core.Interfaces.Runtime
{
    /// <summary>
    /// An instance of this type is constructed and passed to the
    /// element rendering pipeline when a data binding context is
    /// established. When there is no data binding the overhead of
    /// creating a data context is avoided.
    /// </summary>
    public interface IDataContext
    {
        /// <summary>
        /// Returns the Owin Context. This provides access to the request.
        /// You can set response headers etc, but you must not use this to
        /// write to the response stream.
        /// </summary>
        IOwinContext OwinContext { get; }

        /// <summary>
        /// Initializes the render context for a specific request
        /// </summary>
        /// <param name="context"></param>
        IDataContext Initialize(IOwinContext context);

    }
}
