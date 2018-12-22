using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Builder;
using OwinFramework.Interfaces.Builder;
using OwinFramework.Interfaces.Routing;
using OwinFramework.InterfacesV1.Capability;
using OwinFramework.InterfacesV1.Middleware;
using OwinFramework.InterfacesV1.Upstream;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core
{
    /// <summary>
    /// Include this middleware into youe Owin pipeline to render pages and
    /// respond to service requets
    /// </summary>
    public class PagesMiddleware: 
        IMiddleware<IResponseProducer>,
        IRoutingProcessor,
        ISelfDocumenting,
        ITraceable,
        IAnalysable
    {
        /// <summary>
        /// Implements ITraceable
        /// </summary>
        public Action<IOwinContext, Func<string>> Trace { get; set; }

        string IMiddleware.Name { get; set; }

        private readonly IList<IDependency> _dependencies = new List<IDependency>();
        IList<IDependency> IMiddleware.Dependencies { get { return _dependencies; } }

        private readonly IRequestRouter _requestRouter;
        private readonly TimeSpan _maximumCacheTime = TimeSpan.FromHours(1);

        /// <summary>
        /// Constructor for IoC dependency injection
        /// </summary>
        public PagesMiddleware(
            IRequestRouter requestRouter)
        {
            _requestRouter = requestRouter;

            this.RunAfter<IOutputCache>(null, false);
            this.RunAfter<IRequestRewriter>(null, false);
            this.RunAfter<IResponseRewriter>(null, false);
            this.RunAfter<IAuthorization>(null, false);
        }

        Task IRoutingProcessor.RouteRequest(IOwinContext context, Func<Task> next)
        {
            var runable = _requestRouter.Route(context, Trace);

            if (runable == null)
                return next();

            context.SetFeature(runable);

            if (!string.IsNullOrEmpty(runable.RequiredPermission) && 
                string.IsNullOrEmpty(runable.SecureResource))
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

            if (!string.IsNullOrEmpty(runable.CacheCategory))
            {
                var upstreamOutputCache = context.GetFeature<IUpstreamOutputCache>();
                if (upstreamOutputCache.CachedContentIsAvailable)
                {
                    var timeInCache = upstreamOutputCache.TimeInCache;
                    if (timeInCache.HasValue && timeInCache.Value > _maximumCacheTime)
                        upstreamOutputCache.UseCachedContent = false;
                }
            }

            return null;
        }

        Task IMiddleware.Invoke(IOwinContext context, Func<Task> next)
        {
            var runable = context.GetFeature<IRunable>();
            
            if (runable == null)
                return next();

            if (!string.IsNullOrEmpty(runable.RequiredPermission) &&
                !string.IsNullOrEmpty(runable.SecureResource))
            {
                var authorization = context.GetFeature<IAuthorization>();
                if (authorization != null)
                {
                    if (!authorization.HasPermission(runable.RequiredPermission, runable.SecureResource))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        return context.Response.WriteAsync(string.Empty);
                    }
                }
            }

            if (runable.AuthenticationFunc != null)
            {
                if (!runable.AuthenticationFunc(context))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    return context.Response.WriteAsync(string.Empty);
                }
            }

            if (!string.IsNullOrEmpty(runable.CacheCategory))
            {
                var outputCache = context.GetFeature<IOutputCache>();
                if (outputCache != null)
                {
                    outputCache.Category = runable.CacheCategory;
                    outputCache.MaximumCacheTime = _maximumCacheTime;
                    outputCache.Priority = runable.CachePriority;
                    Trace(context, () => GetType().Name + " configured output cache " + outputCache.Category + " " + outputCache.Priority + " " + outputCache.MaximumCacheTime);
                }
            }

            return runable.Run(context, Trace);
        }

        #region Self-documenting

        string ISelfDocumenting.ShortDescription { 
            get { return "An extremely efficient and flexible engine for rendering html and providing restful services"; } }

        string ISelfDocumenting.LongDescription { get { return null; } }

        Uri ISelfDocumenting.GetDocumentation(DocumentationTypes documentationType)
        {
            switch (documentationType)
            {
                case DocumentationTypes.Overview:
                    return new Uri("https://github.com/Bikeman868/OwinFramework.Pages/wiki");
                case DocumentationTypes.SourceCode:
                    return new Uri("https://github.com/Bikeman868/OwinFramework.Pages");
            }
            return null;
        }

        IList<IEndpointDocumentation> ISelfDocumenting.Endpoints
        {
            get { return _requestRouter.GetEndpointDocumentation(); }
        }

        #endregion


        IList<IStatisticInformation> IAnalysable.AvailableStatistics
        {
            get
            {
                var analysable = _requestRouter as IAnalysable;
                return analysable == null 
                    ? new List<IStatisticInformation>() 
                    : analysable.AvailableStatistics;
            }
        }

        IStatistic IAnalysable.GetStatistic(string id)
        {
            var analysable = _requestRouter as IAnalysable;
            return analysable == null
                ? null
                : analysable.GetStatistic(id);
        }
    }
}
