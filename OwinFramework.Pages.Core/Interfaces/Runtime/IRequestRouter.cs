using System;
using System.Collections.Generic;
using Microsoft.Owin;
using OwinFramework.InterfacesV1.Capability;

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

        /// <summary>
        /// Registers a handler for requests that match a filter
        /// </summary>
        /// <param name="runable">The handler to run when the filter is matched</param>
        /// <param name="filter">The filter that matches the request</param>
        /// <param name="priority">Filters are run in ascending order of priority</param>
        /// <param name="declaringType">The type from which this runable was derrived</param>
        void Register(IRunable runable, IRequestFilter filter, int priority = 0, Type declaringType = null);

        /// <summary>
        /// Registers a nested router. Nesting routers makes routing more efficient
        /// and more scaleable
        /// </summary>
        /// <param name="router">The router to run when the filter is matched</param>
        /// <param name="filter">The filter that matches the request</param>
        /// <param name="priority">Filters are run in ascending order of priority</param>
        void Register(IRequestRouter router, IRequestFilter filter, int priority = 0);

        /// <summary>
        /// Returns documentation about the endpoints that are registered with the router
        /// </summary>
        IList<IEndpointDocumentation> GetEndpointDocumentation(); 
    }
}
