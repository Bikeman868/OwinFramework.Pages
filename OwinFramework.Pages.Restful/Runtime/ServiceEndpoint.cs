using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.InterfacesV1.Middleware;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Restful.Interfaces;

namespace OwinFramework.Pages.Restful.Runtime
{
    internal class ServiceEndpoint : IRunable
    {
        public readonly string Path;

        ElementType INamed.ElementType { get { return ElementType.Service; } }
        string INamed.Name { get; set; }

        IPackage IPackagable.Package { get; set; }

        string IRunable.RequiredPermission { get; set; }
        string IRunable.SecureResource { get; set; }
        bool IRunable.AllowAnonymous { get; set; }
        string IRunable.CacheCategory { get; set; }
        Func<IOwinContext, bool> IRunable.AuthenticationFunc { get { return null; } }
        CachePriority IRunable.CachePriority { get; set; }

        public IRequestDeserializer RequestDeserializer { get; set; }
        public IResponseSerializer ResponseSerializer { get; set; }

        private EndpointParameter[] _parameters;

        public ServiceEndpoint(string path)
        {
            Path = path;
        }

        public void AddParameter(string name, EndpointParameterType type, IParameterValidator validator)
        {
            var parameter = new EndpointParameter { Name = name, Type = type, Validator = validator };

            if (_parameters == null)
            {
                _parameters = new[] { parameter };
            }
            else
            {
                var parameters = _parameters.ToList();
                parameters.Add(parameter);
                _parameters = parameters.ToArray();
            }
        }

        Task IRunable.Run(IOwinContext context, Action<IOwinContext, Func<string>> trace)
        {
            trace(context, () => "Executing service endpoint " + Path);

            context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
            return context.Response.WriteAsync("Not implemented yet");
        }

        private class EndpointParameter
        {
            public string Name { get; set; }
            public EndpointParameterType Type { get; set;}
            public IParameterValidator Validator { get; set; }
        }
    }
}