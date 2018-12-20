﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Owin;
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
        public Method[] Methods { get; set; }
        public int RoutingPriority { get; set; }
        public Type DeclaringType { get; set; }
        public Type DefaultDeserializerType { get; set; }
        public Type DefaultSerializerType { get; set; }

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
        }

        public void Initialize()
        {
            var defaultSerializer = GetResponseSerializer(DefaultSerializerType);
            var defaultDeserialzer = GetRequestDeserializer(DefaultDeserializerType);

            object serviceInstance = this;
            if (GetType() == typeof(Service))
            {
                var constructor = DeclaringType.GetConstructor(Type.EmptyTypes);
                if (constructor == null)
                    throw new ServiceBuilderException(
                        "The '" + DeclaringType.DisplayName() + "' service is invalid. You " +
                        "must either provide a default public constructor or inherit from " + 
                        GetType().DisplayName());
                serviceInstance = constructor.Invoke(null);
            }

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
                    if (methodParameters == null || methodParameters.Length != 1)
                        throw new ServiceBuilderException(
                            "The '" + method.Name + "' endpoint of the '" + Name +
                            "' service has the wrong parameter list, it must only take one parameter " +
                            "of type '" + typeof(IEndpointRequest).DisplayName() + "'");

                    if (methodParameters[0].ParameterType != typeof(IEndpointRequest))
                        throw new ServiceBuilderException(
                            "The parameter of the '" + method.Name + "' endpoint in the '" + Name +
                            "' service has the wrong type. The parameter must be " +
                            "of type '" + typeof(IEndpointRequest).DisplayName() + "'");

                    var path = method.Name.ToLower();
                    if (!string.IsNullOrEmpty(endpointAttribute.UrlPath))
                        path = endpointAttribute.UrlPath;

                    var relativePath = !path.StartsWith("/");
                    if (relativePath) path = BasePath + path;

                    var m = method;
                    var endpoint = new ServiceEndpoint(
                        path, 
                        r => m.Invoke(serviceInstance, new[] { r }),
                        m,
                        _serviceDependenciesFactory.DataCatalog,
                        _serviceDependenciesFactory.DataDependencyFactory)
                    {
                        RequestDeserializer = endpointAttribute.RequestDeserializer == null 
                            ? defaultDeserialzer 
                            : GetRequestDeserializer(endpointAttribute.RequestDeserializer),
                        ResponseSerializer = endpointAttribute.ResponseSerializer == null
                            ? defaultSerializer
                            : GetResponseSerializer(endpointAttribute.ResponseSerializer),
                    };

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

                    Register(endpoint, endpointAttribute.MethodsToRoute ?? Methods, relativePath);
                }
            }
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

        private void Register(ServiceEndpoint endpoint, Method[] methods, bool relativePath)
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
                    endpoint.MethodInfo);
            }
            else
            {
                requestRouter.Register(
                    endpoint,
                    new FilterAllFilters(
                        new FilterByMethod(methods),
                        new FilterByPath(pathFilter)),
                    RoutingPriority,
                    endpoint.MethodInfo);
            }
        }
    }
}
