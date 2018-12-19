using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Owin;
using OwinFramework.InterfacesV1.Capability;
using OwinFramework.MiddlewareHelpers.SelfDocumenting;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces.Capability;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Framework.Runtime
{
    internal class RequestRouter: IRequestRouter
    {
        private readonly List<Registration> _registrations = new List<Registration>();

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

        IRequestRouter IRequestRouter.Add(IRequestFilter filter, int priority)
        {
            var router = new RequestRouter();
            ((IRequestRouter)this).Register(router, filter, priority);
            return router;
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

        IDisposable IRequestRouter.Register(IRunable runable, IRequestFilter filter, int priority, MethodInfo methodInfo)
        {
            var registration = new Registration
            {
                Priority = priority,
                Filter = filter,
                Runable = runable,
                DeclaringType = runable.GetType(),
                Method = methodInfo
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
                var endpointDocumentation = new EndpointDocumentation
                {
                    RelativePath = registration.Filter.Description
                };

                if (registration.Router != null)
                {
                    var routerEndpoints = registration.Router.GetEndpointDocumentation();
                    endpoints.AddRange(routerEndpoints);
                    continue;
                }

                var documented = registration.Runable as IDocumented;
                if (documented != null)
                {
                    endpointDocumentation.Description = documented.Description;
                    endpointDocumentation.Examples = documented.Examples;
                    endpointDocumentation.Attributes = documented.Attributes;
                }

                object[] customAttributes = null;
                if(registration.Method != null)
                    customAttributes = registration.Method.GetCustomAttributes(true);
                else if (registration.DeclaringType != null)
                    customAttributes = registration.DeclaringType.GetCustomAttributes(true);

                string endpointPath = null;
                string queryStringExample = null;

                if (customAttributes != null)
                {
                    foreach (var attribute in customAttributes)
                    {
                        var description = attribute as DescriptionAttribute;
                        if (description != null)
                        {
                            endpointDocumentation.Description = endpointDocumentation.Description == null
                                ? description.Html
                                : (endpointDocumentation.Description + "<br>" + description.Html);
                        }

                        var example = attribute as ExampleAttribute;
                        if (example != null)
                        {
                            endpointDocumentation.Examples = endpointDocumentation.Examples == null
                                ? example.Html
                                : (endpointDocumentation.Examples + "<br>" + example.Html);
                        }

                        var option = attribute as OptionAttribute;
                        if (option != null)
                        {
                            if (endpointDocumentation.Attributes == null)
                                endpointDocumentation.Attributes = new List<IEndpointAttributeDocumentation>();

                            endpointDocumentation.Attributes.Add(new EndpointAttributeDocumentation
                                {
                                    Type = option.OptionType.ToString(),
                                    Name = option.Name,
                                    Description = option.Html
                                });
                        }

                        var endpoint = attribute as EndpointAttribute;
                        if (endpoint != null)
                        {
                            endpointPath = endpoint.UrlPath;
                        }

                        var endpointParameter = attribute as EndpointParameterAttribute;
                        if (endpointParameter != null)
                        {
                            if (endpointDocumentation.Attributes == null)
                                endpointDocumentation.Attributes = new List<IEndpointAttributeDocumentation>();

                            var parameterValue = "{" + endpointParameter.ParameterName + "}";
                            var parameterDescription = endpointParameter.ParserType.DisplayName();

                            if (typeof(IDocumented).IsAssignableFrom(endpointParameter.ParserType) || 
                                typeof(IParameterParser).IsAssignableFrom(endpointParameter.ParserType))
                            {
                                var constructor = endpointParameter.ParserType.GetConstructor(Type.EmptyTypes);
                                if (constructor != null)
                                {
                                    var parser = constructor.Invoke(null);
                                    var parameterDocumented = parser as IDocumented;
                                    var parameterParser = parser as IParameterParser;

                                    if (parameterDocumented != null)
                                    {
                                        parameterDescription = parameterDocumented.Description;
                                        parameterValue = parameterDocumented.Examples;
                                    }
                                    else if (parameterParser != null)
                                        parameterDescription = parameterParser.Description;
                                }
                            }

                            if (endpointParameter.ParameterType.HasFlag(EndpointParameterType.QueryString))
                            {
                                if (queryStringExample == null)
                                    queryStringExample = "?" + endpointParameter.ParameterName + "=" + parameterValue;
                                else
                                    queryStringExample += "&" + endpointParameter.ParameterName + "=" + parameterValue;
                            }

                            endpointDocumentation.Attributes.Add(new EndpointAttributeDocumentation
                            {
                                Type = endpointParameter.ParameterType.ToString(),
                                Name = endpointParameter.ParameterName,
                                Description = parameterDescription
                            });
                        }
                    }
                }

                if (string.IsNullOrEmpty(endpointDocumentation.Examples) && !string.IsNullOrEmpty(endpointPath))
                {
                    endpointDocumentation.Examples = endpointPath;
                    if (!string.IsNullOrEmpty(queryStringExample))
                        endpointDocumentation.Examples += queryStringExample;
                }

                endpoints.Add(endpointDocumentation);
            }

            return endpoints;
        }

        private class Registration: IComparable, IDisposable
        {
            public int Priority;
            public IRequestFilter Filter;
            public IRunable Runable;
            public Type DeclaringType;
            public MethodInfo Method;
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
    }
}
