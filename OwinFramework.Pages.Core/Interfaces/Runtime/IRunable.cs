using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.InterfacesV1.Middleware;

namespace OwinFramework.Pages.Core.Interfaces.Runtime
{
    /// <summary>
    /// This is implemented by elements that can hndle requests and return responses
    /// </summary>
    public interface IRunable
    {
        /// <summary>
        /// Returns the name of a permission that the caller must have, or null 
        /// to skip this check.
        /// </summary>
        string RequiredPermission { get; set; }

        /// <summary>
        /// Optional resource name to supply when checking the required permission. See
        /// authorization middleware documentation for more details.
        /// </summary>
        string SecureResource { get; set; }

        /// <summary>
        /// Returns a flag indicating if anonymous users are allowed to request this
        /// resource
        /// </summary>
        bool AllowAnonymous { get; set; }

        /// <summary>
        /// The category name to use with the output cache
        /// </summary>
        string CacheCategory { get; set; }

        /// <summary>
        /// The priority to use with the output cache
        /// </summary>
        CachePriority CachePriority { get; set; }

        /// <summary>
        /// Optional authentication function. If this is not null then it will be called
        /// to verrify the user has permission before invoking the runable. If this function
        /// returns false then a 403 response is returned to the caller
        /// </summary>
        Func<IOwinContext, bool> AuthenticationFunc { get; }

        /// <summary>
        /// Handles the request and returns a response
        /// </summary>
        Task Run(IOwinContext context, Action<IOwinContext, Func<string>> trace);
    }
}
