using Microsoft.Owin;
using OwinFramework.Builder;
using OwinFramework.Interfaces.Builder;
using OwinFramework.Interfaces.Routing;
using OwinFramework.InterfacesV1.Capability;
using OwinFramework.InterfacesV1.Middleware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Sample5.Middleware
{
    public class RedirectMiddleware : IMiddleware<IRequestRewriter>, ITraceable, IRoutingProcessor
    {
        string IMiddleware.Name { get; set; }

        private readonly IList<IDependency> _dependencies = new List<IDependency>();
        IList<IDependency> IMiddleware.Dependencies { get { return _dependencies; } }

        public Action<IOwinContext, Func<string>> Trace { get; set; }

        private Tuple<PathString, PathString>[] _redirects;

        public RedirectMiddleware()
        {
            _redirects = new Tuple<PathString, PathString>[]
            {
                new Tuple<PathString, PathString>(new PathString("/favicon.ico"), new PathString("/images/favicon.ico"))
            };
        }

        public Task RouteRequest(IOwinContext context, Func<Task> next)
        {
            for (var i = 0; i < _redirects.Length; i++)
                if (_redirects[i].Item1.Equals(context.Request.Path))
                {
                    context.Request.Path = _redirects[i].Item2;
                    Trace(context, () => "Rewriting " + _redirects[i].Item1 + " to " + _redirects[i].Item2);
                    break;
                }
            return next();
        }

        public Task Invoke(IOwinContext context, Func<Task> next)
        {
            return next();
        }
    }
}