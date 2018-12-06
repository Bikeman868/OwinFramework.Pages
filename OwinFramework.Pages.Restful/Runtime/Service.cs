using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.InterfacesV1.Middleware;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Core.RequestFilters;

namespace OwinFramework.Pages.Restful.Runtime
{
    /// <summary>
    /// Base implementation of IComponent. Inheriting from this olass will insulate you
    /// from any future additions to the IComponent interface
    /// </summary>
    public class Service : IService, IDebuggable
    {
        private readonly IServiceDependenciesFactory _serviceDependenciesFactory;
        public ElementType ElementType { get { return ElementType.Service; } }

        public string Name { get; set; }
        public IPackage Package { get; set; }
        public string RequiredPermission { get; set; }
        public string SecureResource { get; set; }
        public bool AllowAnonymous { get; set; }
        public Func<IOwinContext, bool> AuthenticationFunc { get { return null; } }
        public string CacheCategory { get; set; }
        public CachePriority CachePriority { get; set; }
        public IModule Module { get; set; }
        public string BasePath { get; set; }
        public bool EndpointSpecificPermission { get; set; }
        public Methods[] Methods { get; set; }
        public int RoutingPriority { get; set; }
        public Type DeclaringType { get; set; }
        public Type DefaultDeserializerType { get; set; }
        public Type DefaultSerializerType { get; set; }

        private readonly Regex _paramRegex = new Regex("{[^}]*}", RegexOptions.Compiled | RegexOptions.Singleline);

        public Service(IServiceDependenciesFactory serviceDependenciesFactory)
        {
            _serviceDependenciesFactory = serviceDependenciesFactory;
        }

        public void Initialize()
        {
            // TODO: Use reflection to discover endpoints
            // TODO: Register endpoints

            var endpoint = new Endpoint(BasePath + "test");
            var runable = (IRunable)endpoint;

            runable.Name = "test";

            runable.RequiredPermission = RequiredPermission;
            if (EndpointSpecificPermission)
                runable.SecureResource = Name + "/" + runable.Name;

            Methods[] methods = null;

            Register(endpoint, methods ?? Methods);
        }

        T IDebuggable.GetDebugInfo<T>(int parentDepth, int childDepth)
        {
            return new DebugService() as T;
        }

        private void Register(Endpoint endpoint, Methods[] methods)
        {
            var pathFilter = _paramRegex.Replace(endpoint.Path, "*");

            if (methods == null || methods.Length == 0)
            {
                _serviceDependenciesFactory.RequestRouter.Register(
                    endpoint, 
                    new FilterByPath(pathFilter), 
                    RoutingPriority, 
                    DeclaringType);
            }
            else
            {
                _serviceDependenciesFactory.RequestRouter.Register(
                    endpoint,
                    new FilterAllFilters(
                        new FilterByMethod(methods),
                        new FilterByPath(pathFilter)),
                    RoutingPriority,
                    DeclaringType);
            }
        }

        private class Endpoint: IRunable
        {
            public string Path;

            ElementType INamed.ElementType { get { return ElementType.Service; } }
            string INamed.Name { get; set; }

            IPackage IPackagable.Package { get; set; }

            string IRunable.RequiredPermission { get; set; }
            string IRunable.SecureResource { get; set; }
            bool IRunable.AllowAnonymous { get; set; }
            string IRunable.CacheCategory { get; set; }
            Func<IOwinContext, bool> IRunable.AuthenticationFunc { get { return null; } }
            CachePriority IRunable.CachePriority { get; set; }

            public Endpoint(string path)
            {
                Path = path;
            }

            Task IRunable.Run(IOwinContext context, Action<IOwinContext, Func<string>> trace)
            {
                trace(context, () => "Executing service endpoint " + Path);

                context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
                return context.Response.WriteAsync("Not implemented yet");
            }
        }
    }
}
