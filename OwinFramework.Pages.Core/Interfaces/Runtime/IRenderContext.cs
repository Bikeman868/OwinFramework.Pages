using System;
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
        /// Call this method to output trace information only when tracing
        /// is enabled for the request. When request tracing is turned off
        /// the function does nothing.
        /// </summary>
        /// <param name="messageFunc">A function that returns a message
        /// to include in the trace output. If tracing is turned off then
        /// this function will not be called, so you must not rely on
        /// any side effects of executing this function</param>
        void Trace(Func<string> messageFunc);

        /// <summary>
        /// Call this method to output trace information only when tracing
        /// is enabled for the request. When request tracing is turned off
        /// the function does nothing.
        /// </summary>
        /// <param name="messageFunc">A function that returns a message
        /// to include in the trace output. If tracing is turned off then
        /// this function will not be called, so you must not rely on
        /// any side effects of executing this function</param>
        /// <param name="arg">An argument to pass to the message function</param>
        void Trace<T>(Func<T, string> messageFunc, T arg);

        /// <summary>
        /// Call this to cause the trace output to be indented. Be sure to
        /// balance each call with a call to TraceOutdent
        /// </summary>
        void TraceIndent();

        /// <summary>
        /// Call this to cause the trace output to be no longer indented. Call
        /// this once for each call to TraceIndent();
        /// </summary>
        void TraceOutdent();

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
        /// Gets and sets the data context that should be used for the current operation
        /// </summary>
        IDataContext Data { get; set; }

        /// <summary>
        /// Writes a script open tag for the page initialization area. Calling this
        /// multiple times for the same page only renders one script open
        /// </summary>
        void EnsureInitializationArea();

        /// <summary>
        /// Writes a single script close tag only if the EnsureInitializationArea
        /// method was called at least once
        /// </summary>
        void EndInitializationArea();

        /// <summary>
        /// Adds a request specific data context that is linked to a specific page,
        /// service or region.
        /// </summary>
        /// <param name="id">A unique ID for this data context</param>
        /// <param name="dataContext">A data context specific to this request</param>
        void AddDataContext(int id, IDataContext dataContext);

        /// <summary>
        /// Gets the data context identified by a unique id
        /// </summary>
        IDataContext GetDataContext(int id);

        /// <summary>
        /// Switches the current data context based on unique id
        /// </summary>
        /// <param name="id">The id of the data context to make current</param>
        void SelectDataContext(int id);

        /// <summary>
        /// Disposes of all data associated with this render context
        /// </summary>
        void DeleteDataContextTree();
    }
}
