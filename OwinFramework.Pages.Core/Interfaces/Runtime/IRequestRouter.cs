using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Owin;
using OwinFramework.InterfacesV1.Capability;
using OwinFramework.Pages.Core.Enums;

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
        /// <param name="context">The OWIN context for the request</param>
        /// <param name="trace">The method of outputting trace information for this request</param>
        /// <param name="rewritePath">If this is an internal rewrite then this is the path to rewrite. Null for the original request</param>
        /// <param name="rewriteMethod">If this is an internal rewrite then this is the method to rewrite</param>
        IRunable Route(
            IOwinContext context, 
            Action<IOwinContext, Func<string>> trace, 
            string rewritePath = null, 
            Method rewriteMethod = Method.Get);

        /// <summary>
        /// Adds a branch to the routing tree. This is more efficient
        /// than having a single router with a long list of filters
        /// </summary>
        /// <param name="filter">The filter that matches the request</param>
        /// <param name="priority">Filters are run in ascending order of priority</param>
        /// <param name="scenarioName">Optional name that identifies the A/B testing scenario that
        /// this route applies to</param>
        IRequestRouter Add(IRequestFilter filter, int priority = 0, string scenarioName = null);

        /// <summary>
        /// Registers a handler for requests that match a filter
        /// </summary>
        /// <param name="runable">The handler to run when the filter is matched</param>
        /// <param name="filter">The filter that matches the request</param>
        /// <param name="priority">Filters are run in ascending order of priority</param>
        /// <param name="declaringType">The type from which this runable was derrived. 
        /// This is used to find attributes that contain documentation</param>
        /// <param name="scenarioName">Optional name that identifies the A/B testing scenario that
        /// this route applies to</param>
        /// <returns>A disposable instance. Disposing of this instance will de-register
        /// this route. The router keeps a reference to this instance so you don't need to</returns>
        IDisposable Register(IRunable runable, IRequestFilter filter, int priority = 0, Type declaringType = null, string scenarioName = null);

        /// <summary>
        /// Registers a handler for requests that match a filter
        /// </summary>
        /// <param name="runable">The handler to run when the filter is matched</param>
        /// <param name="filter">The filter that matches the request</param>
        /// <param name="priority">Filters are run in ascending order of priority</param>
        /// <param name="methodInfo">A method that is decorated with custom documentation attributes</param>
        /// <param name="scenarioName">Optional name that identifies the A/B testing scenario that
        /// this route applies to</param>
        /// <returns>A disposable instance. Disposing of this instance will de-register
        /// this route. The router keeps a reference to this instance so you don't need to</returns>
        IDisposable Register(IRunable runable, IRequestFilter filter, int priority, MethodInfo methodInfo, string scenarioName);

        /// <summary>
        /// Registers a nested router. Nesting routers makes routing more efficient
        /// and more scaleable
        /// </summary>
        /// <param name="router">The router to run when the filter is matched</param>
        /// <param name="filter">The filter that matches the request</param>
        /// <param name="priority">Filters are run in ascending order of priority</param>
        /// <param name="scenarioName">Optional name that identifies the A/B testing scenario that
        /// this route applies to</param>
        /// <returns>A disposable instance. Disposing of this instance will de-register
        /// this route. The router keeps a reference to this instance so you don't need to</returns>
        IDisposable Register(IRequestRouter router, IRequestFilter filter, int priority = 0, string scenarioName = null);

        /// <summary>
        /// Returns documentation about the endpoints that are registered with the router
        /// </summary>
        IList<IEndpointDocumentation> GetEndpointDocumentation(); 
    }
}
