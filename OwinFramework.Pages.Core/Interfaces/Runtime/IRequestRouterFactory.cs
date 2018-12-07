using System;
using System.Collections.Generic;
using Microsoft.Owin;
using OwinFramework.InterfacesV1.Capability;

namespace OwinFramework.Pages.Core.Interfaces.Runtime
{
    /// <summary>
    /// A factory for request routers
    /// </summary>
    public interface IRequestRouterFactory
    {
        /// <summary>
        /// Creates a new instance that can route requests based on filter expressions
        /// </summary>
        IRequestRouter Create();
    }
}
