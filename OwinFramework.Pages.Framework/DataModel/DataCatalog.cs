using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;

namespace OwinFramework.Pages.Framework.DataModel
{
    internal class DataCatalog: IDataCatalog
    {
        private readonly HashSet<string> _assemblies;
        private readonly HashSet<Type> _types;
        private readonly IThreadSafeDictionary<Type, SupplierRegistration[]> _registrations;

        public DataCatalog(
            IDictionaryFactory dictionaryFactory)
        {
            _assemblies = new HashSet<string>();
            _types = new HashSet<Type>();
            _registrations = dictionaryFactory.Create<Type, SupplierRegistration[]>();
        }

        public IDataCatalog Register(IDataSupplier dataSupplier)
        {
            if (dataSupplier == null)
                return this;

            if (!_types.Add(dataSupplier.GetType()))
                return this;

            foreach (var type in dataSupplier.SuppliedTypes)
            {
                lock(_registrations)
                {
                    SupplierRegistration[] registrations;
                    if (_registrations.TryGetValue(type, out registrations))
                    {
                        var newArray = new SupplierRegistration[registrations.Length + 1];
                        registrations.CopyTo(newArray, 0);
                        newArray[newArray.Length - 1] = new SupplierRegistration(dataSupplier, type);
                        _registrations[type] = newArray;
                    }
                    else
                    {
                        _registrations.Add(type, new[] { new SupplierRegistration(dataSupplier, type) });
                    }
                }
            }

            return this;
        }

        public IDataCatalog Register(Type dataSupplierType, Func<Type, object> factoryFunc)
        {
            if (_types.Contains(dataSupplierType))
                return this;

            if (!typeof(IDataSupplier).IsAssignableFrom(dataSupplierType))
                throw new NotImplementedException(dataSupplierType.DisplayName() +
                    " can not be registered as a data provider because it does not implement the IDataSupplier interface");

            var factoryInstance = factoryFunc(dataSupplierType);

            if (factoryInstance == null)
                throw new NotImplementedException(dataSupplierType.DisplayName() +
                    " can not be registered as a data supplier because the factory function did not return an instance");

            var dataSupplier = factoryInstance as IDataSupplier;

            if (dataSupplier == null)
                throw new NotImplementedException(dataSupplierType.DisplayName() +
                    " can not be registered as a data supplier because the instance constructed by the factory function does not implement IDataSupplier");

            return Register(dataSupplier);
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

        public IDataSupplier FindSupplier(IDataDependency dependency)
        {
            var type = dependency.DataType;

            if (type == null)
                throw new Exception("Data dependencies must include the data type thay are dependent on");

            SupplierRegistration[] registrations;
            if (!_registrations.TryGetValue(type, out registrations))
                return null;

            SupplierRegistration unscopedRegistration = null;
            SupplierRegistration scopedRegistration = null;

            for (var i = 0; i < registrations.Length; i++)
            {
                var registration = registrations[i];
                var supplier = registration.Supplier;
                if (supplier.IsSupplierOf(dependency))
                {
                    if (registration.IsScoped)
                    {
                        if (scopedRegistration == null || scopedRegistration.DependencyScore > registration.DependencyScore)
                            scopedRegistration = registration;
                    }
                    else
                    {
                        if (unscopedRegistration == null || unscopedRegistration.DependencyScore > registration.DependencyScore)
                            unscopedRegistration = registration;
                    }
                }
            }

            if (string.IsNullOrEmpty(dependency.ScopeName))
            {
                return unscopedRegistration == null
                    ? (scopedRegistration == null ? null : scopedRegistration.Supplier)
                    : unscopedRegistration.Supplier;
            }

            return scopedRegistration == null
                ? (unscopedRegistration == null ? null : unscopedRegistration.Supplier)
                : scopedRegistration.Supplier;
        }

        private class SupplierRegistration
        {
            public readonly IDataSupplier Supplier;
            public readonly bool IsScoped;
            public readonly int DependencyScore;

            public SupplierRegistration(IDataSupplier supplier, Type type)
            {
                Supplier = supplier;
                IsScoped = supplier.IsScoped(type);

                var consumer = supplier as IDataConsumer;
                if (consumer != null)
                {
                    var needs = consumer.GetConsumerNeeds();
                    if (needs != null)
                    {
                        if (needs.DataSupplyDependencies != null)
                            DependencyScore += needs.DataSupplyDependencies.Count;

                        if (needs.DataSupplierDependencies != null)
                            DependencyScore += needs.DataSupplierDependencies.Count * 2;

                        if (needs.DataDependencies != null)
                            DependencyScore += needs.DataDependencies.Count * 3;
                    }
                }
            }
        }
    }
}
