using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        string RequiredPermission { get; }

        /// <summary>
        /// Returns a flag indicating if anonymous users are allowed to request this
        /// resource
        /// </summary>
        bool AllowAnonymous { get; }

        /// <summary>
        /// Optional authentication function. If this is not null then it will be called
        /// to verrify the user has permission before invoking the runable. If this function
        /// returns false then a 403 response is returned to the caller
        /// </summary>
        Func<IOwinContext, bool> AuthenticationFunc { get; }

        /// <summary>
        /// Handles the request and returns a response
        /// </summary>
        Task Run(IOwinContext context);
    }
}
