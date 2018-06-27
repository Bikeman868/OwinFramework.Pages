﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;

namespace OwinFramework.Pages.Framework.DataModel
{
    internal class DataCatalog: IDataCatalog
    {
        private readonly IDictionaryFactory _dictionaryFactory;
        private readonly INameManager _nameManager;
        private readonly HashSet<string> _assemblies;
        private readonly HashSet<string> _types;
        private readonly List<Registration> _providers;
        private readonly IThreadSafeDictionary<Type, List<Registration>> _providersByType;

        private class Registration : IDataProviderRegistration
        {
            public IDataProvider DataProvider { get; set; }
            public int Priority { get; set; }
            public IList<string> Scopes { get; private set; }
            public IList<Type> Types { get; private set; }
            public IList<IDataProvider> DependentProviders { get; private set; }

            public readonly IList<string> Names = new List<string>();
            public readonly IList<string> DependentProviderNames = new List<string>();
            public readonly IList<Type> DependentTypes = new List<Type>();

            public Registration()
            {
                Scopes = new List<string>();
                Types = new List<Type>();
                DependentProviders = new List<IDataProvider>();
            }
        }

        private class RegistrationComparer: IComparer<Registration>
        {
            int IComparer<Registration>.Compare(Registration x, Registration y)
            {
                if (x.Priority < y.Priority) return -1;
                if (x.Priority > y.Priority) return 1;
                return 0;
            }
        }

        // Algorithm:
        // Does the data already exist anywhere in the tree
        // Is the scope null
        //    Is there a provider for this type of data and no scope
        //    Is there a provider for this type of data with any scope
        // Is there a scope
        //   Is there a provider for this type of data and this scope
        //   Move up to parent scope and try again
        //  

        public DataCatalog(
            IDictionaryFactory dictionaryFactory,
            INameManager nameManager)
        {
            _dictionaryFactory = dictionaryFactory;
            _nameManager = nameManager;
            _assemblies = new HashSet<string>();
            _types = new HashSet<string>();
            _providers = new List<Registration>();
            _providersByType = dictionaryFactory.Create<Type, List<Registration>>();

            nameManager.AddResolutionHandler(() =>
                { 
                    foreach (var provider in _providers)
                    {
                        foreach(var name in provider.DependentProviderNames)
                        {
                            provider.DependentProviders.Add(_nameManager.ResolveDataProvider(name, provider.DataProvider.Package));
                        }
                    }

                    var comparer = new RegistrationComparer();
                    foreach(var type in _providersByType.Keys)
                    {
                        var providers = _providersByType[type];
                        providers.Sort(comparer);
                    }
                });
        }

        public IDataCatalog Register(IDataProvider dataProvider)
        {
            if (dataProvider == null)
                return this;

            if (!_types.Add(dataProvider.GetType().FullName))
                return this;

            var registration = ProcessCustomAttributes(dataProvider);
            _providers.Add(registration);

            if (registration.Scopes == null || registration.Scopes.Count == 0)
                registration.Priority += 4;
            else if (registration.Scopes.Count > 1)
                registration.Priority += 1;

            if (registration.Types.Count > 1)
                registration.Priority += 2;

            foreach (var type in registration.Types)
            {
                var providers = _providersByType.GetOrAdd(type, s => new List<Registration>(), null);
                providers.Add(registration);
            }

            return this;
        }

        public IDataCatalog Register(Type dataProviderType, Func<Type, object> factoryFunc)
        {
            if (_types.Contains(dataProviderType.FullName))
                return this;

            if (!typeof(IDataProvider).IsAssignableFrom(dataProviderType))
                throw new NotImplementedException(dataProviderType.FullName + 
                    " can not be registered as a data provider because it does not implement the IDataProvider interface");

            var factoryInstance = factoryFunc(dataProviderType);

            if (factoryInstance == null)
                throw new NotImplementedException(dataProviderType.FullName +
                    " can not be registered as a data provider because the factory function did not return an instance");

            var dataProvider = factoryInstance as IDataProvider;

            if (dataProvider == null)
                throw new NotImplementedException(dataProviderType.FullName +
                    " can not be registered as a data provider because the instance constructed by the factory function does not implement IDataProvider");

            return Register(dataProvider);
        }

        public IDataCatalog Register(Assembly assembly, Func<Type, object> factoryFunc)
        {
            if (!_assemblies.Add(assembly.FullName))
                return this;

            var types = assembly.GetTypes();

            var dataProviderTypes = types.Where(t => t.GetCustomAttributes(true).Any(a => a is IsDataProviderAttribute));

            var exceptions = new List<Exception>();

            foreach (var providerType in dataProviderTypes)
            {
                try
                {
                    Register(providerType, factoryFunc);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Count == 1)
                throw exceptions[0];

            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);

            return this;
        }

        private Registration ProcessCustomAttributes(IDataProvider dataProvider)
        {
            var registration = new Registration { DataProvider = dataProvider };

            foreach (var attribute in dataProvider.GetType().GetCustomAttributes(true))
            {
                var isDataProvider = attribute as IsDataProviderAttribute;
                var needsData = attribute as NeedsDataAttribute;
                var partOf = attribute as PartOfAttribute;

                if (isDataProvider != null)
                {
                    if (!string.IsNullOrEmpty(isDataProvider.Name))
                    {
                        dataProvider.Name = isDataProvider.Name;
                        registration.Names.Add(isDataProvider.Name);
                    }

                    if (!string.IsNullOrEmpty(isDataProvider.Scope))
                        registration.Scopes.Add(isDataProvider.Scope);

                    if (isDataProvider.Type != null)
                        registration.Types.Add(isDataProvider.Type);
                }

                if (needsData != null)
                {
                    if (!string.IsNullOrEmpty(needsData.DataProviderName))
                        registration.DependentProviderNames.Add(needsData.DataProviderName);

                    if (needsData.DataType != null)
                        registration.DependentTypes.Add(needsData.DataType);
                }

                if (partOf != null)
                {
                    if (!string.IsNullOrEmpty(partOf.PackageName))
                        dataProvider.Package = _nameManager.ResolvePackage(partOf.PackageName);
                }

            }

            foreach (var name in registration.Names)
            {
                _nameManager.Register(registration.DataProvider, name);
            }

            return registration;
        }

        public IDataProviderRegistration FindProvider(IDataDependency dependency)
        {
            if (dependency.DataType == null)
                throw new Exception("Data dependencies must include the data type thay are dependent on");

            List<Registration> registrations;
            if (!_providersByType.TryGetValue(dependency.DataType, out registrations))
                return null;

            Registration registration = null;
            if (!string.IsNullOrEmpty(dependency.ScopeName))
            {
                registration = registrations
                    .FirstOrDefault(r => r.Scopes != null && r.Scopes.Any(s => string.Equals(s, dependency.ScopeName, StringComparison.OrdinalIgnoreCase)));
            }

            if (registration == null)
                registration = registrations
                    .FirstOrDefault(r => r.Scopes == null || r.Scopes.Count == 0);

            return registration;
        }
    }
}