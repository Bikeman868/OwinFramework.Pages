using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.InterfacesV1.Capability;
using OwinFramework.InterfacesV1.Middleware;
using OwinFramework.MiddlewareHelpers.SelfDocumenting;
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
using OwinFramework.Pages.Restful.Serializers;

namespace OwinFramework.Pages.Restful.Runtime
{
    /// <summary>
    /// Base implementation of IComponent. Inheriting from this olass will insulate you
    /// from any future additions to the IComponent interface
    /// </summary>
    public class Service : IService, IDebuggable, IRequestRouter
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

        private readonly List<Registration> _registrations = new List<Registration>();
        private bool _isServiceRegistered;

        private static readonly IDictionary<Type, IRequestDeserializer> RequestDeserializers = new Dictionary<Type, IRequestDeserializer>();
        private static readonly IDictionary<Type, IResponseSerializer> ResponseSerializers = new Dictionary<Type, IResponseSerializer>();

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

            var methods = DeclaringType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
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
                    var path = method.Name.ToLower();
                    if (!string.IsNullOrEmpty(endpointAttribute.UrlPath))
                        path = endpointAttribute.UrlPath;

                    var relativePath = !path.StartsWith("/");
                    if (relativePath) path = BasePath + path;

                    var endpoint = new Endpoint(path)
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

        private void Register(Endpoint endpoint, Methods[] methods, bool relativePath)
        {
            var requestRouter = _serviceDependenciesFactory.RequestRouter;

            if (relativePath)
            {
                if (!_isServiceRegistered)
                {
                    _isServiceRegistered = true;

                    if (Methods == null || Methods.Length == 0)
                    {
                        _serviceDependenciesFactory.RequestRouter.Register(
                            this,
                            new FilterByPath(BasePath + "**"),
                            RoutingPriority);
                    }
                    else
                    {
                        _serviceDependenciesFactory.RequestRouter.Register(
                            this,
                            new FilterAllFilters(
                                new FilterByMethod(Methods),
                                new FilterByPath(BasePath + "**")),
                            RoutingPriority);
                    }
                }

                requestRouter = this;
            }

            var pathFilter = _paramRegex.Replace(endpoint.Path, "*");

            if (methods == null || methods.Length == 0)
            {
                requestRouter.Register(
                    endpoint, 
                    new FilterByPath(pathFilter), 
                    RoutingPriority, 
                    DeclaringType);
            }
            else
            {
                requestRouter.Register(
                    endpoint,
                    new FilterAllFilters(
                        new FilterByMethod(methods),
                        new FilterByPath(pathFilter)),
                    RoutingPriority,
                    DeclaringType);
            }
        }

        IRunable IRequestRouter.Route(IOwinContext context)
        {
            Registration registration;

            lock (_registrations)
            {
                registration = _registrations.FirstOrDefault(r => r.Filter.IsMatch(context));
            }

            if (registration == null)
                return null;

            return registration.Router == null
                ? registration.Runable
                : registration.Router.Route(context);
        }

        IDisposable IRequestRouter.Register(IRunable runable, IRequestFilter filter, int priority, Type declaringType)
        {
            var registration = new Registration
            {
                Priority = priority,
                Filter = filter,
                Runable = runable,
                DeclaringType = declaringType ?? runable.GetType()
            };

            lock (_registrations)
            {
                _registrations.Add(registration);
                _registrations.Sort();
            }

            return registration;
        }

        IDisposable IRequestRouter.Register(IRequestRouter router, IRequestFilter filter, int priority)
        {
            var registration = new Registration
            {
                Priority = priority,
                Filter = filter,
                Router = router
            };

            lock (_registrations)
            {
                _registrations.Add(registration);
                _registrations.Sort();
            }

            return registration;
        }

        IList<IEndpointDocumentation> IRequestRouter.GetEndpointDocumentation()
        {
            List<Registration> registrations;
            lock (_registrations)
                registrations = _registrations.ToList();

            var endpoints = new List<IEndpointDocumentation>();

            foreach (var registration in registrations)
            {
                var endpoint = new EndpointDocumentation
                {
                    RelativePath = registration.Filter.Description
                };

                if (registration.Router != null)
                {
                    var routerEndpoints = registration.Router.GetEndpointDocumentation();
                    endpoints.AddRange(routerEndpoints);
                    continue;
                }

                if (registration.Runable != null)
                {
                    var documented = registration.Runable as IDocumented;
                    if (documented != null)
                    {
                        endpoint.Description = documented.Description;
                        endpoint.Examples = documented.Examples;
                        endpoint.Attributes = documented.Attributes;
                    }
                }

                if (registration.DeclaringType != null)
                {
                    foreach (var attribute in registration.DeclaringType.GetCustomAttributes(true))
                    {
                        var description = attribute as DescriptionAttribute;
                        if (description != null)
                        {
                            endpoint.Description = endpoint.Description == null
                                ? description.Html
                                : (endpoint.Description + "<br>" + description.Html);
                        }

                        var example = attribute as ExampleAttribute;
                        if (example != null)
                        {
                            endpoint.Examples = endpoint.Examples == null
                                ? example.Html
                                : (endpoint.Examples + "<br>" + example.Html);
                        }

                        var option = attribute as OptionAttribute;
                        if (option != null)
                        {
                            if (endpoint.Attributes == null)
                                endpoint.Attributes = new List<IEndpointAttributeDocumentation>();

                            endpoint.Attributes.Add(new EndpointAttributeDocumentation
                            {
                                Type = option.OptionType.ToString(),
                                Name = option.Name,
                                Description = option.Html
                            });
                        }
                    }
                }

                endpoints.Add(endpoint);
            }

            return endpoints;
        }

        private class Registration : IComparable, IDisposable
        {
            public int Priority;
            public IRequestFilter Filter;
            public IRunable Runable;
            public Type DeclaringType;
            public IRequestRouter Router;

            int IComparable.CompareTo(object obj)
            {
                var other = obj as Registration;
                if (other == null) return 1;

                if (Priority == other.Priority) return 0;

                return Priority > other.Priority ? -1 : 1;
            }

            public void Dispose()
            {
                //TODO: De-register on dispose
            }
        }

        private class Endpoint : IRunable
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

            public IRequestDeserializer RequestDeserializer { get; set; }
            public IResponseSerializer ResponseSerializer { get; set; }

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
