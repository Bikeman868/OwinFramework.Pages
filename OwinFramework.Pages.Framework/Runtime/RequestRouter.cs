using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using OwinFramework.InterfacesV1.Capability;
using OwinFramework.MiddlewareHelpers.SelfDocumenting;
using OwinFramework.Pages.Core.Attributes;
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

        private class Registration: IComparable, IDisposable
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
    }
}
