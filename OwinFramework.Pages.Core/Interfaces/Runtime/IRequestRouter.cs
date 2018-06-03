using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwinFramework.Pages.Core.Interfaces.Runtime
{
    /// <summary>
    /// The request router figures out which page or service will handle
    /// the incomming request.
    /// </summary>
    public interface IRequestRouter
    {
        /// <summary>
        /// Takes an incomming request and decides which page or service
        /// will handle it. Returns null for unrecognized URLs
        /// </summary>
        IRunable Route(IOwinContext context);
    }
}
