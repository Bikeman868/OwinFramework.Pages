using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.Owin;
using OwinFramework.InterfacesV1.Capability;
using OwinFramework.InterfacesV1.Middleware;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Capability;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Core.RequestFilters;
using OwinFramework.Pages.Restful.Interfaces;
using OwinFramework.Pages.Restful.Parameters;
using OwinFramework.Pages.Restful.Serializers;
using System.Text;

namespace OwinFramework.Pages.Restful.Runtime
{
    /// <summary>
    /// Base implementation of IService. Inheriting from this class will insulate you
    /// from any future additions to the IService interface
    /// </summary>
    public class Service : IService, IDebuggable, IAnalysable
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
        public Method[] Methods { get; set; }
        public int RoutingPriority { get; set; }
        public Type DeclaringType { get; set; }
        public Type DefaultDeserializerType { get; set; }
        public Type DefaultSerializerType { get; set; }
        public string ClientScriptComponentName { get; set; }
        public string ClientScript { get; set; }

        private readonly Regex _paramRegex = new Regex("{[^}]*}", RegexOptions.Compiled | RegexOptions.Singleline);

        private IRequestRouter _router;

        private static readonly IDictionary<Type, IRequestDeserializer> RequestDeserializers = new Dictionary<Type, IRequestDeserializer>();
        private static readonly IDictionary<Type, IResponseSerializer> ResponseSerializers = new Dictionary<Type, IResponseSerializer>();
        private static readonly IDictionary<Type, IParameterParser> ParameterParsers = new Dictionary<Type, IParameterParser>();

        /// <summary>
        /// Constructs a new service that can 
        /// - route requests to service endpoints
        /// - deserialize request bodies
        /// - extract, parse and validate endpoint parameters
        /// - serialize the response
        /// </summary>
        public Service(IServiceDependenciesFactory serviceDependenciesFactory)
        {
            _serviceDependenciesFactory = serviceDependenciesFactory;

            DefaultDeserializerType = typeof(Json);
            DefaultSerializerType = typeof(Json);
            AllowAnonymous = true;
        }

        public void Initialize(Func<Type, object> factory)
        {
            var defaultSerializer = GetResponseSerializer(DefaultSerializerType);
            var defaultDeserialzer = GetRequestDeserializer(DefaultDeserializerType);

            object serviceInstance = this;
            if (GetType() == typeof(Service))
            {
                if (factory == null)
                {
                    var constructor = DeclaringType.GetConstructor(Type.EmptyTypes);

                    if (constructor == null)
                        throw new ServiceBuilderException(
                            "The '" + DeclaringType.DisplayName() + "' service is invalid. You " +
                            "must either supply a factory method, provide a default public constructor, or inherit from " +
                            GetType().DisplayName());

                    serviceInstance = constructor.Invoke(null);
                }
                else
                {
                    serviceInstance = factory(DeclaringType);
                }
            }

            var clientScript = new StringBuilder();
            clientScript.AppendLine("return {");
            var firstEndpoint = true;

            var methods = serviceInstance.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var method in methods)
            {
                EndpointAttribute endpointAttribute = null;
                var parameterAttributes = new List<EndpointParameterAttribute>();

                foreach(var attribute in method.GetCustomAttributes(false))
                {
                    if (attribute is EndpointAttribute)
                        endpointAttribute = (EndpointAttribute)attribute;
                    else if (attribute is EndpointParameterAttribute)
                        parameterAttributes.Add((EndpointParameterAttribute)attribute);
                }

                if (endpointAttribute != null)
                {
                    if (method.ReturnType != typeof(void))
                        throw new ServiceBuilderException(
                            "The '" + method.Name + "' endpoint of the '" + Name +
                            "' service has a return type, but it should have a return type of 'void'");

                    var methodParameters = method.GetParameters();

                    if (methodParameters.Length == 0 || methodParameters[0].ParameterType != typeof(IEndpointRequest))
                        throw new ServiceBuilderException(
                            "The '" + method.Name + "' endpoint of the '" + Name +
                            "' service has the wrong parameter list, the first parameter must be " +
                            "of type '" + typeof(IEndpointRequest).DisplayName() + "'");

                    var parameterNames = new string[methodParameters.Length];

                    if (methodParameters.Length > 1)
                    {
                        for (var i = 1; i < methodParameters.Length; i++)
                        {
                            var methodParameter = methodParameters[i];
                            EndpointParameterAttribute endpointParameterAttribute = null;

                            foreach (var attribute in methodParameter.GetCustomAttributes(false))
                            {
                                endpointParameterAttribute = attribute as EndpointParameterAttribute;
                                if (endpointParameterAttribute != null)
                                {
                                    if (string.IsNullOrEmpty(endpointParameterAttribute.ParameterName))
                                        endpointParameterAttribute.ParameterName = methodParameter.Name;

                                    if (endpointParameterAttribute.ParserType == null)
                                        endpointParameterAttribute.ParserType = methodParameter.ParameterType;

                                    break;
                                }
                            }

                            if (endpointParameterAttribute == null)
                            {
                                endpointParameterAttribute = new EndpointParameterAttribute 
                                {
                                    ParameterName = methodParameter.Name,
                                    ParameterType = EndpointParameterType.QueryString,
                                    ParserType = methodParameter.ParameterType
                                };
                            }

                            parameterAttributes.Add(endpointParameterAttribute);
                            parameterNames[i] = endpointParameterAttribute.ParameterName;
                        }
                    }

                    var path = method.Name.ToLower();
                    if (!string.IsNullOrEmpty(endpointAttribute.UrlPath))
                        path = endpointAttribute.UrlPath;

                    var relativePath = !path.StartsWith("/");
                    if (relativePath) path = BasePath + path;

                    var m = method;

                    Action<IEndpointRequest> action;
                    if (methodParameters.Length == 1)
                        action = r => m.Invoke(serviceInstance, new[] { r });
                    else
                        action = r => 
                        {
                            var parameters = new object[parameterNames.Length];

                            parameters[0] = r;

                            for (var i = 1; i < parameterNames.Length; i++)
                                parameters[i] = r.GetParameter(parameterNames[i]);

                            m.Invoke(serviceInstance, parameters);
                        };

                    var httpMethods = endpointAttribute.Methods ?? Methods;

                    var endpoint = new ServiceEndpoint(
                        path,
                        httpMethods,
                        action,
                        m,
                        endpointAttribute.UserSegmentKey,
                        endpointAttribute.Analytics,
                        _serviceDependenciesFactory.DataCatalog,
                        _serviceDependenciesFactory.DataDependencyFactory,
                        _serviceDependenciesFactory.RequestRouter)
                    {
                        RequestDeserializer = endpointAttribute.RequestDeserializer == null 
                            ? defaultDeserialzer 
                            : GetRequestDeserializer(endpointAttribute.RequestDeserializer),
                        ResponseSerializer = endpointAttribute.ResponseSerializer == null
                            ? defaultSerializer
                            : GetResponseSerializer(endpointAttribute.ResponseSerializer),
                    };

                    AddAnalysable(endpoint);

                    var runable = (IRunable)endpoint;
                    runable.Name = method.Name;
                    runable.AllowAnonymous = AllowAnonymous;
                    runable.CacheCategory = CacheCategory;
                    runable.CachePriority = CachePriority;
                    runable.Package = Package;

                    if (endpointAttribute.RequiredPermission == null)
                    {
                        if (!string.IsNullOrEmpty(RequiredPermission))
                        {
                            runable.RequiredPermission = RequiredPermission;
                            if (EndpointSpecificPermission)
                                runable.SecureResource = Name + "/" + runable.Name;
                        }
                    }
                    else
                    {
                        runable.RequiredPermission = endpointAttribute.RequiredPermission;
                    }

                    foreach(var parameter in parameterAttributes)
                    {
                        var parameterParser = GetParameterParser(parameter.ParserType);
                        endpoint.AddParameter(parameter.ParameterName, parameter.ParameterType, parameterParser);
                    }

                    Register(endpoint, httpMethods, relativePath, endpointAttribute.UserSegmentKey);

                    if (!firstEndpoint) clientScript.AppendLine("  },");
                    firstEndpoint = false;

                    var queryStringParameters = parameterAttributes
                        .Where(p => (p.ParameterType & EndpointParameterType.QueryString) == EndpointParameterType.QueryString)
                        .Select(p => p.ParameterName)
                        .ToList();

                    var pathParameters = parameterAttributes
                        .Where(p => (p.ParameterType & (EndpointParameterType.PathSegment | EndpointParameterType.QueryString)) == EndpointParameterType.PathSegment)
                        .Select(p => p.ParameterName)
                        .ToList();

                    var headerParameters = parameterAttributes
                        .Where(p => (p.ParameterType & (EndpointParameterType.Header | EndpointParameterType.PathSegment | EndpointParameterType.QueryString)) == EndpointParameterType.Header)
                        .Select(p => p.ParameterName)
                        .ToList();

                    var formParameters = parameterAttributes
                        .Where(p => (p.ParameterType & (EndpointParameterType.FormField | EndpointParameterType.Header | EndpointParameterType.PathSegment | EndpointParameterType.QueryString)) == EndpointParameterType.FormField)
                        .Select(p => p.ParameterName)
                        .ToList();

                    var methodName = char.ToLower(method.Name[0]) + method.Name.Substring(1);
                    clientScript.AppendLine("  " + methodName + ": function(params, onSuccess, onDone, onFail) {");
                    clientScript.AppendLine("    var request = { isSuccess: function(ajax){ return ajax.status === 200; } };");
                    clientScript.AppendLine("    if (params != undefined && params.body != undefined) request.body = params.body;");

                    if (pathParameters.Count > 0)
                    {
                        var url = "\"" + path.ToLower() + "\"";
                        foreach (var parameter in pathParameters)
                            url = url.Replace("{" + parameter.ToLower() + "}", "\" + encodeURIComponent(params." + parameter + ") + \"");
                        if (url.EndsWith(" + \"\"")) url = url.Substring(0, url.Length - 5);
                        if (url.StartsWith("\"\" + ")) url = url.Substring(5);
                        clientScript.AppendLine("    request.url = " + url + ";");
                    }
                    else
                    {
                        clientScript.AppendLine("    request.url = \"" + path + "\";");
                    }

                    if (queryStringParameters.Count > 0)
                    {
                        clientScript.AppendLine("    var query = \"\";");
                        clientScript.AppendLine("    if (params != undefined) {");
                        foreach (var parameter in queryStringParameters)
                            clientScript.AppendLine("      if (params." + parameter + " != undefined) query += \"&" + parameter + "=\" + encodeURIComponent(params." + parameter + ");");
                        clientScript.AppendLine("    }");
                        clientScript.AppendLine("    if (query.length > 0) request.url += \"?\" + query.substring(1);");
                    }

                    if (headerParameters.Count > 0)
                    {
                        clientScript.AppendLine("    request.headers = [");
                        for (var i = 0; i < headerParameters.Count; i++)
                        {
                            var headerParameter = headerParameters[i];
                            clientScript.AppendLine("      { name: \"" + headerParameter + "\", value: params." + headerParameter + " }" + (i == headerParameters.Count - 1 ? "" : ","));
                        }
                        clientScript.AppendLine("    ];");
                    }

                    if (formParameters.Count > 0)
                    {
                        clientScript.AppendLine("    if (params != undefined) {");
                        clientScript.AppendLine("      var form = \"\";");
                        foreach (var parameter in formParameters)
                          clientScript.AppendLine("      if (params." + parameter + " != undefined) form += \"&" + parameter + "=\" + encodeURIComponent(params." + parameter + ");");
                        clientScript.AppendLine("      if (form.length > 0) {");
                        clientScript.AppendLine("        request.body = form.substring(1);");
                        clientScript.AppendLine("      }");
                        clientScript.AppendLine("    }");
                    }

                    clientScript.AppendLine("    if (onSuccess != undefined) request.onSuccess = function(ajax){ onSuccess(ajax.response); }");
                    clientScript.AppendLine("    if (onFail != undefined) request.onFail = onFail;");
                    clientScript.AppendLine("    if (onDone != undefined) request.onDone = onDone;");

                    for (var i = httpMethods.Length - 1; i >= 0; i--)
                    {
                        var httpMethod = httpMethods[i];
                        var functionCall = string.Empty;

                        switch (httpMethod)
                        {
                            case Method.Get:
                                functionCall = "ns.ajax.restModule.getJson(request)";
                                break;
                            case Method.Post:
                                functionCall = formParameters.Count > 0 ? "ns.ajax.restModule.postForm(request)" : "ns.ajax.restModule.postJson(request)";
                                break;
                            case Method.Put:
                                functionCall = formParameters.Count > 0 ? "ns.ajax.restModule.putForm(request)" : "ns.ajax.restModule.putJson(request)";
                                break;
                            case Method.Delete:
                                functionCall = "ns.ajax.restModule.sendDelete(request)";
                                break;
                        }

                        if (httpMethods.Length == 1)
                            clientScript.AppendLine("    " + functionCall + ";");
                        else if (i == 0)
                            clientScript.AppendLine("    else " + functionCall + ";");
                        else if (i == httpMethods.Length - 1)
                            clientScript.AppendLine("    if (params.method == \"" + httpMethod.ToString().ToUpper() + "\") " + functionCall + ";");
                        else
                            clientScript.AppendLine("    else if (params.method == \"" + httpMethod.ToString().ToUpper() + "\") " + functionCall + ";");
                    }
                }
            }

            if (!firstEndpoint) clientScript.AppendLine("  }");
            clientScript.AppendLine("}");
            ClientScript = clientScript.ToString();
        }

        T IDebuggable.GetDebugInfo<T>(int parentDepth, int childDepth)
        {
            return new DebugService() as T;
        }

        private IRequestDeserializer GetRequestDeserializer(Type type)
        {
            lock(RequestDeserializers)
            {
                IRequestDeserializer deserializer;
                if (RequestDeserializers.TryGetValue(type, out deserializer))
                    return deserializer;

                if (!typeof(IRequestDeserializer).IsAssignableFrom(type))
                    throw new ServiceBuilderException(
                        "Type " + type.DisplayName() + " was configured as a deserializer in " +
                        "the '" + Name + "' service, but it does not implement IRequestDeserializer");

                var constructor = type.GetConstructor(Type.EmptyTypes);
                if (constructor == null)
                    throw new ServiceBuilderException(
                        "Type " + type.DisplayName() + " was configured as a deserializer in " +
                        "the '" + Name + "' service, but it does not have a default public constructor");

                try
                {
                    deserializer = constructor.Invoke(null) as IRequestDeserializer;
                }
                catch (Exception ex)
                {
                    throw new ServiceBuilderException(
                        "The default public constructor for request deserializer " + 
                        type.DisplayName() + " threw exception " + ex.Message, ex);
                }

                RequestDeserializers[type] = deserializer;
                return deserializer;
            }
        }

        private IResponseSerializer GetResponseSerializer(Type type)
        {
            lock (ResponseSerializers)
            {
                IResponseSerializer serializer;
                if (ResponseSerializers.TryGetValue(type, out serializer))
                    return serializer;

                if (!typeof(IResponseSerializer).IsAssignableFrom(type))
                    throw new ServiceBuilderException(
                        "Type " + type.DisplayName() + " was configured as a serializer in " +
                        "the '" + Name + "' service, but it does not implement IResponseSerializer");

                var constructor = type.GetConstructor(Type.EmptyTypes);
                if (constructor == null)
                    throw new ServiceBuilderException(
                        "Type " + type.DisplayName() + " was configured as a serializer in " +
                        "the '" + Name + "' service, but it does not have a default public constructor");

                try
                {
                    serializer = constructor.Invoke(null) as IResponseSerializer;
                }
                catch (Exception ex)
                {
                    throw new ServiceBuilderException(
                        "The default public constructor for response serializer " +
                        type.DisplayName() + " threw exception " + ex.Message, ex);
                }

                ResponseSerializers[type] = serializer;
                return serializer;
            }
        }

        private IParameterParser GetParameterParser(Type type)
        {
            lock (ParameterParsers)
            {
                IParameterParser parser;
                if (ParameterParsers.TryGetValue(type, out parser))
                    return parser;

                if (typeof(IParameterParser).IsAssignableFrom(type))
                {
                    var constructor = type.GetConstructor(Type.EmptyTypes);
                    if (constructor == null)
                        throw new ServiceBuilderException(
                            "Type " + type.DisplayName() + " was configured as a service endpoint parameter parser in " +
                            "the '" + Name + "' service, but it does not have a default public constructor");

                    try
                    {
                        parser = constructor.Invoke(null) as IParameterParser;
                    }
                    catch (Exception ex)
                    {
                        throw new ServiceBuilderException(
                            "The default public constructor for parameter parser '" +
                            type.DisplayName() + "' threw exception " + ex.Message, ex);
                    }
                }
                else
                {
                    parser = new ParameterParser(type);
                }

                ParameterParsers[type] = parser;
                return parser;
            }
        }

        private void Register(ServiceEndpoint endpoint, Method[] methods, bool relativePath, string userSegmentKey)
        {
            var requestRouter = _serviceDependenciesFactory.RequestRouter;

            if (relativePath)
            {
                if (_router == null)
                {
                    if (Methods == null || Methods.Length == 0)
                    {
                        _router = _serviceDependenciesFactory.RequestRouter.Add(
                            new FilterByPath(BasePath + "**"),
                            RoutingPriority);
                    }
                    else
                    {
                        _router = _serviceDependenciesFactory.RequestRouter.Add(
                            new FilterAllFilters(
                                new FilterByMethod(Methods),
                                new FilterByPath(BasePath + "**")),
                            RoutingPriority);
                    }
                }

                requestRouter = _router;
            }

            var pathFilter = _paramRegex.Replace(endpoint.Path, "*");

            if (methods == null || methods.Length == 0)
            {
                requestRouter.Register(
                    endpoint, 
                    new FilterByPath(pathFilter), 
                    RoutingPriority, 
                    endpoint.MethodInfo,
                    userSegmentKey);
            }
            else
            {
                requestRouter.Register(
                    endpoint,
                    new FilterAllFilters(
                        new FilterByMethod(methods),
                        new FilterByPath(pathFilter)),
                    RoutingPriority,
                    endpoint.MethodInfo,
                    userSegmentKey);
            }
        }

        #region IAnalysable

        private readonly List<IAnalysable> _analysableEndpoints = new List<IAnalysable>();
        private readonly Dictionary<string, IAnalysable> _endpointStatistics = new Dictionary<string, IAnalysable>();

        private void AddAnalysable(IAnalysable analysable)
        {
            if (analysable == null) return;

            lock (_analysableEndpoints)
                _analysableEndpoints.Add(analysable);

            lock (_endpointStatistics)
            {
                foreach (var stat in analysable.AvailableStatistics)
                {
                    _endpointStatistics.Add(stat.Id, analysable);
                }
            }
        }

        public IList<IStatisticInformation> AvailableStatistics
        {
            get
            {
                lock (_analysableEndpoints)
                    return _analysableEndpoints.SelectMany(a => a.AvailableStatistics).ToList();
            }
        }

        public IStatistic GetStatistic(string id)
        {
            IAnalysable analysable;
            bool found;

            lock (_endpointStatistics)
                found = _endpointStatistics.TryGetValue(id, out analysable);

            return found ? analysable.GetStatistic(id) : null;
        }

        #endregion
    }
}
