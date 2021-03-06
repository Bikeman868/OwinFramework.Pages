﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Owin;
using OwinFramework.InterfacesV1.Capability;
using OwinFramework.MiddlewareHelpers.SelfDocumenting;
using OwinFramework.Pages.Core;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces.Capability;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Core.Interfaces.Segmentation;
using OwinFramework.Pages.Framework.Analytics;

namespace OwinFramework.Pages.Framework.Runtime
{
    internal class RequestRouter: IRequestRouter, IAnalysable
    {
        private Registration[] _registrations = new Registration[0];
        private readonly object _registrationlock = new object();

        IRunable IRequestRouter.Route(
            IOwinContext context, 
            Action<IOwinContext, Func<string>> trace,
            string rewritePath,
            Method rewriteMethod)
        {
            string url;
            string method;

            if (rewritePath == null)
            {
                url = context.Request.Path.Value;
                method = context.Request.Method;
            }
            else
            {
                if (!rewritePath.StartsWith("/"))
                    throw new Exception("When rewriting requests an absolute path must be provided: '" + rewritePath + "'");

                url = rewritePath;
                method = rewriteMethod.ToString().ToUpper();
                trace(context, () => "Rewriting the request to " + method + " " + rewritePath);
            }

            Registration registration;

            var testScenario = context.Get<ISegmentTestingScenario>(EnvironmentKeys.TestScenario);
            if (testScenario == null)
            {
                registration = _registrations.FirstOrDefault(r => r.TestScenarioName == null && r.Filter.IsMatch(context, url, method));
            }
            else
            {
                trace(context, () => "Routing to the '" + testScenario.Name + "' test scenario");
                registration = _registrations.FirstOrDefault(r => (r.TestScenarioName == testScenario.Name || r.TestScenarioName == null) && r.Filter.IsMatch(context, url, method));
            }

            if (registration == null)
            {
                trace(context, () => "No Pages Framework filters match the request");
                return null;
            }

            if (registration.Router == null)
            {
                trace(context, () => 
                    "Pages framework router matched runable " + 
                    registration.DeclaringType.DisplayName() + " with filter " +
                    registration.Filter.Description);
                return registration.Runable;
            }

            trace(context, () => 
                "Pages framework router matched a further level of routing with filter " + 
                registration.Filter.Description);

            return registration.Router.Route(context, trace, rewritePath, rewriteMethod);
        }

        IRequestRouter IRequestRouter.Add(IRequestFilter filter, int priority, string scenarioName)
        {
            var router = new RequestRouter();
            ((IRequestRouter)this).Register(router, filter, priority, scenarioName);
            return router;
        }

        IDisposable IRequestRouter.Register(
            IRunable runable, 
            IRequestFilter filter, 
            int priority, 
            Type declaringType,
            string scenarioName)
        {
            return Add(new Registration
                {
                    Priority = priority,
                    Filter = filter,
                    TestScenarioName = string.IsNullOrWhiteSpace(scenarioName) ? null : scenarioName,
                    Runable = runable,
                    DeclaringType = declaringType ?? runable.GetType()
                });
        }

        IDisposable IRequestRouter.Register(
            IRunable runable, 
            IRequestFilter filter, 
            int priority, 
            MethodInfo methodInfo,
            string scenarioName)
        {
            return Add(new Registration
                {
                    Priority = priority,
                    Filter = filter,
                    TestScenarioName = string.IsNullOrWhiteSpace(scenarioName) ? null : scenarioName,
                    Runable = runable,
                    DeclaringType = runable.GetType(),
                    Method = methodInfo
                });
        }

        IDisposable IRequestRouter.Register(
            IRequestRouter router, 
            IRequestFilter filter, 
            int priority,
            string scenarioName)
        {
            return Add(new Registration
                {
                    Priority = priority,
                    Filter = filter,
                    TestScenarioName = string.IsNullOrWhiteSpace(scenarioName) ? null : scenarioName,
                    Router = router
                });
        }

        IList<IEndpointDocumentation> IRequestRouter.GetEndpointDocumentation()
        {
            List<Registration> registrations;
            lock (_registrations)
                registrations = _registrations.ToList();

            var endpoints = new List<IEndpointDocumentation>();

            foreach (var registration in registrations.Where(r => r.TestScenarioName == null))
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
                if (registration.Method != null)
                {
                    customAttributes = registration.Method.GetCustomAttributes(true)
                        .Concat(
                            registration.Method.GetParameters()
                                .Skip(1)
                                .Select(p => new
                                    {
                                        parameter = p,
                                        attribute = p.GetCustomAttributes(false).Select(a => a as EndpointParameterAttribute).FirstOrDefault(a => a != null)
                                    })
                                .Select(o => 
                                    {
                                        var attribute = o.attribute;

                                        if (attribute == null) 
                                            attribute = new EndpointParameterAttribute
                                            {
                                                ParameterType = EndpointParameterType.QueryString
                                            };

                                        if (string.IsNullOrEmpty(attribute.ParameterName))
                                            attribute.ParameterName = o.parameter.Name;

                                        if (attribute.ParserType == null)
                                            attribute.ParserType = o.parameter.ParameterType;

                                        return attribute;
                                    }))
                        .ToArray();
                }
                else if (registration.DeclaringType != null)
                {
                    customAttributes = registration.DeclaringType.GetCustomAttributes(true);
                }

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
                                if (!string.IsNullOrEmpty((endpointParameter.Description)))
                                    parameterDescription = endpointParameter.Description;
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

        #region IAnalysable

        private Dictionary<string, EndpointStatistic> _endpointStatistics;

        private void AddAnalysable(IAnalysable analysable)
        {
            if (analysable == null) return;

            foreach (var statistic in analysable.AvailableStatistics)
            {
                var s = statistic;
                _endpointStatistics.Add(
                    statistic.Id,
                    new EndpointStatistic
                    {
                        Statistic = statistic,
                        GetStatistic = () => analysable.GetStatistic(s.Id)
                    });
            }
        }

        public IList<IStatisticInformation> AvailableStatistics
        {
            get
            {
                var endpointStatistics = new Dictionary<string, EndpointStatistic>();
                foreach (var analysable in _registrations.Select(r => r.Runable == null ? (r.Router as IAnalysable) : (r.Runable as IAnalysable)).Where(a => a != null))
                {
                    foreach (var statistic in analysable.AvailableStatistics)
                    {
                        var s = statistic;
                        endpointStatistics.Add(
                            statistic.Id,
                            new EndpointStatistic
                            {
                                Statistic = statistic,
                                GetStatistic = () => analysable.GetStatistic(s.Id)
                            });
                    }
                }
                _endpointStatistics = endpointStatistics;

                lock (_endpointStatistics)
                    return _endpointStatistics.Values.Select(a => a.Statistic).ToList();
            }
        }

        public IStatistic GetStatistic(string id)
        {
            EndpointStatistic statistic;
            bool found;

            lock (_endpointStatistics)
                found = _endpointStatistics.TryGetValue(id, out statistic);

            return found ? statistic.GetStatistic() : null;
        }

        #endregion

        private Registration Add(Registration registration)
        {
            if (registration.TestScenarioName != null)
                registration.Priority += 1;

            lock (_registrationlock)
            {
                var list = _registrations.ToList();
                list.Add(registration);
                list.Sort();
                _registrations = list.ToArray();
            }

            return registration;
        }

        private class Registration : IComparable, IDisposable
        {
            public int Priority;
            public string TestScenarioName;
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
