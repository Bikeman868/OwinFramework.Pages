using System;
using System.Collections.Generic;
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
using OwinFramework.Pages.Core.Interfaces.Capability;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Restful.Interfaces;

namespace OwinFramework.Pages.Restful.Runtime
{
    internal class ServiceEndpoint : IRunable
    {
        public readonly string Path;
        public readonly MethodInfo MethodInfo;

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
        private readonly string[] _pathElements;

        public ServiceEndpoint(
            string path, 
            Action<IEndpointRequest> method,
            MethodInfo methodInfo,
            IDataCatalog dataCatalog,
            IDataDependencyFactory dataDependencyFactory)
        {
            Path = path;
            MethodInfo = methodInfo;
            _method = method;
            _dataCatalog = dataCatalog;
            _dataDependencyFactory = dataDependencyFactory;
            _pathElements = path
                .Split('/')
                .Where(p => !string.IsNullOrEmpty(p))
                .ToArray();
        }

        public void AddParameter(string name, EndpointParameterType parameterType, IParameterParser parser)
        {
            var functions = new List<Func<IEndpointRequest, string, string>>();

            if (parameterType.HasFlag(EndpointParameterType.QueryString))
                functions.Add(QueryStringParam);

            if (parameterType.HasFlag(EndpointParameterType.Header))
                functions.Add(HeaderParam);

            if (parameterType.HasFlag(EndpointParameterType.PathSegment))
            {
                var placeholder = "{" + name + "}";
                for (var i = 0; i < _pathElements.Length; i++)
                {
                    var index = i;
                    if (string.Equals(placeholder, _pathElements[i], StringComparison.OrdinalIgnoreCase))
                    {
                        functions.Add((r, n) => PathSegmentParam(r, index));
                        break;
                    }
                }
            }

            if (parameterType.HasFlag(EndpointParameterType.FormField))
                functions.Add(FormFieldParam);

            var parameter = new EndpointParameter 
            { 
                Name = name, 
                ParameterType = parameterType, 
                Parser = parser,
                Functions = functions.ToArray()
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

        private string QueryStringParam(IEndpointRequest request, string parameterName)
        {
            return request.OwinContext.Request.Query[parameterName];
        }

        private string HeaderParam(IEndpointRequest request, string parameterName)
        {
            return request.OwinContext.Request.Headers[parameterName];
        }

        private string PathSegmentParam(IEndpointRequest request, int pathIndex)
        {
            return request.PathSegment(pathIndex);
        }

        private string FormFieldParam(IEndpointRequest request, string parameterName)
        {
            var form = request.Form;
            if (form == null) return null;

            var values = form.GetValues(parameterName);
            if (values == null || values.Count == 0)
                return null;

            return values[0];
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
                    request.HttpStatus(HttpStatusCode.BadRequest, "Parameter '" + e.ParameterName + "' is invalid. " + e.ValidationError);
                }
                catch(BodyDeserializationException e)
                {
                    trace(context, () =>
                        "Request body should by '" + e.BodyType.DisplayName() + "'. " + e.ValidationError +
                        (string.IsNullOrEmpty(e.StackTrace) ? string.Empty : "\n" + e.StackTrace));
                    request.HttpStatus(HttpStatusCode.BadRequest, "Request body is invalid. " + e.ValidationError);
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