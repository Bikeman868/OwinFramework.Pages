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
        private readonly Action<IEndpointRequest> _method;

        public ServiceEndpoint(string path, Action<IEndpointRequest> method)
        {
            Path = path;
            _method = method;
        }

        public void AddParameter(string name, EndpointParameterType parameterType, IParameterValidator validator)
        {
            var parameter = new EndpointParameter 
            { 
                Name = name, 
                ParameterType = parameterType, 
                Validator = validator 
            };

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

            using (var request = new EndpointRequest(context))
            {
                try
                {
                    _method(request);
                }
                catch (NotImplementedException e)
                {
                    request.HttpStatus(HttpStatusCode.NotImplemented, "Not implemented yet");
                    return context.Response.WriteAsync(e.Message);
                }
                return request.WriteResponse();
            }
        }

        private class EndpointParameter
        {
            public string Name { get; set; }
            public EndpointParameterType ParameterType { get; set;}
            public IParameterValidator Validator { get; set; }
        }
    }
}