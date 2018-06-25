using Microsoft.Owin;
using OwinFramework.Pages.Core.Interfaces.DataModel;

namespace OwinFramework.Pages.Core.Interfaces.Runtime
{
    /// <summary>
    /// An instance of this interface is associated with each request and has
    /// the same lifetime as the request processing
    /// </summary>
    public interface IRenderContext
    {
        /// <summary>
        /// Initializes the render context for a specific request
        /// </summary>
        IRenderContext Initialize(IOwinContext context);

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
        /// Returns a flag indicating if the html should inclide comments
        /// </summary>
        bool IncludeComments { get; }

        /// <summary>
        /// Returns the data context that should be used for the current operation
        /// </summary>
        IDataContext Data { get; }

        /// <summary>
        /// Adds a request specific data context that is linked to a specific page,
        /// service or region.
        /// </summary>
        /// <param name="id">A unique ID for this data context</param>
        /// <param name="dataContext">A data context specific to this request</param>
        void AddDataContext(int id, IDataContext dataContext);

        /// <summary>
        /// Switches the current data context based on unique id
        /// </summary>
        /// <param name="id">The id of the data context to make current</param>
        void SelectDataContext(int id);
    }
}
