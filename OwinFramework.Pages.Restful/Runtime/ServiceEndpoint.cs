using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.InterfacesV1.Middleware;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Restful.Interfaces;
using OwinFramework.Pages.Restful.Parameters;

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
        private readonly IDataCatalog _dataCatalog;
        private readonly IDataDependencyFactory _dataDependencyFactory;

        public ServiceEndpoint(
            string path, 
            Action<IEndpointRequest> method,
            IDataCatalog dataCatalog,
            IDataDependencyFactory dataDependencyFactory)
        {
            Path = path;
            _method = method;
            _dataCatalog = dataCatalog;
            _dataDependencyFactory = dataDependencyFactory;
        }

        public void AddParameter(string name, EndpointParameterType parameterType, IParameterValidator validator)
        {
            var parameter = new EndpointParameter 
            { 
                Name = name, 
                ParameterType = parameterType, 
                Validator = validator,
                Functions = new Func<IEndpointRequest, string, string>[] { ParamFunc }
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

        private string ParamFunc(IEndpointRequest request, string parameterName)
        {
            return "1";
        }

        Task IRunable.Run(IOwinContext context, Action<IOwinContext, Func<string>> trace)
        {
            trace(context, () => "Executing service endpoint " + Path);

            using (var request = new EndpointRequest(
                context, 
                _dataCatalog,
                _dataDependencyFactory,
                RequestDeserializer, 
                ResponseSerializer,
                _parameters))
            {
                try
                {
                    try
                    {
                        _method(request);
                    }
                    catch (TargetInvocationException e)
                    {
                        trace(context, () => "Service endpoint threw an exception");
                        throw e.InnerException;
                    }
                }
                catch (NotImplementedException e)
                {
                    trace(context, () => 
                        "Not Implemented exception: " + 
                        e.Message + (string.IsNullOrEmpty(e.StackTrace) ? string.Empty : "\n" + e.StackTrace));
                    request.HttpStatus(HttpStatusCode.NotImplemented, "Not implemented yet");
                }
                catch (AggregateException ex)
                {
                    trace(context, () => "Multiple exceptions...");
                    foreach(var e in ex.InnerExceptions)
                    {
                        trace(context, () => 
                            e.GetType().DisplayName() + " exception: " + e.Message + 
                            (string.IsNullOrEmpty(e.StackTrace) ? string.Empty : "\n" + e.StackTrace));
                    }
                    request.HttpStatus(HttpStatusCode.InternalServerError, "Multiple exceptions");
                }
                catch (EndpointParameterException e)
                {
                    trace(context, () =>
                        "Invalid parameter '" + e.ParameterName + "' " + e.ValidationError +
                        (string.IsNullOrEmpty(e.StackTrace) ? string.Empty : "\n" + e.StackTrace));
                    request.HttpStatus(HttpStatusCode.BadRequest, "Parameter '" + e.ParameterName + "' is invalid. " + e.Description);
                }
                catch (Exception e)
                {
                    trace(context, () => 
                        e.GetType().DisplayName() + " exception: " + e.Message + 
                        (string.IsNullOrEmpty(e.StackTrace) ? string.Empty : "\n" + e.StackTrace));
                    request.HttpStatus(HttpStatusCode.InternalServerError, "Unhandled exception");
                }
                return request.WriteResponse();
            }
        }
    }
}