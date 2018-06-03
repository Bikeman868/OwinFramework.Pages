using Microsoft.Owin;
using OwinFramework.Builder;
using OwinFramework.Interfaces.Builder;
using OwinFramework.Interfaces.Routing;
using OwinFramework.InterfacesV1.Middleware;
using OwinFramework.InterfacesV1.Upstream;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OwinFramework.Pages.Core
{
    /// <summary>
    /// Include this middleware into youe Owin pipeline to render pages and
    /// respond to service requets
    /// </summary>
    public class PagesMiddleware: 
        IMiddleware<IResponseProducer>,
        IRoutingProcessor
    {
        string IMiddleware.Name { get; set; }

        private IList<IDependency> _dependencies = new List<IDependency>();
        IList<IDependency> IMiddleware.Dependencies{ 
            get { return _dependencies; }
        }

        private IRequestRouter _requestRouter;

        /// <summary>
        /// Constructor for IoC dependency injection
        /// </summary>
        public PagesMiddleware(
            IRequestRouter requestRouter)
        {
            _requestRouter = requestRouter;
        }

        Task IRoutingProcessor.RouteRequest(IOwinContext context, Func<Task> next)
        {
            var runable = _requestRouter.Route(context);

            if (runable == null)
                return next();

            context.SetFeature(runable);

            if (!string.IsNullOrEmpty(runable.RequiredPermission))
            {
                var upstreamAuthorization = context.GetFeature<IUpstreamAuthorization>();
                if (upstreamAuthorization != null)
                    upstreamAuthorization.AddRequiredPermission(runable.RequiredPermission);
            }

            if (!runable.AllowAnonymous)
            {
                var upstreamIdentification = context.GetFeature<IUpstreamIdentification>();
                if (upstreamIdentification != null)
                    upstreamIdentification.AllowAnonymous = false;
            }

            return null;
        }

        Task IMiddleware.Invoke(IOwinContext context, Func<Task> next)
        {
            var runable = context.GetFeature<IRunable>();
            
            if (runable == null)
                return next();

            if (runable.AuthenticationFunc != null)
            {
                if (!runable.AuthenticationFunc(context))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    return context.Response.WriteAsync(string.Empty);
                }
            }

            return runable.Run(context);
        }
    
   }
}
