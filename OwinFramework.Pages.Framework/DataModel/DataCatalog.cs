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
        private readonly IThreadSafeDictionary<Type, List<IDataSupplier>> _suppliers;

        public DataCatalog(
            IDictionaryFactory dictionaryFactory)
        {
            _assemblies = new HashSet<string>();
            _types = new HashSet<Type>();
            _suppliers = dictionaryFactory.Create<Type, List<IDataSupplier>>();
        }

        public IDataCatalog Register(IDataSupplier dataSupplier)
        {
            if (dataSupplier == null)
                return this;

            if (!_types.Add(dataSupplier.GetType()))
                return this;

            foreach (var type in dataSupplier.SuppliedTypes)
            {
                var suppliers = _suppliers.GetOrAdd(type, t => new List<IDataSupplier>(), null);
                lock(suppliers) suppliers.Add(dataSupplier);
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
            if (dependency.DataType == null)
                throw new Exception("Data dependencies must include the data type thay are dependent on");

            List<IDataSupplier> suppliers;
            if (!_suppliers.TryGetValue(dependency.DataType, out suppliers))
                return null;

            IDataSupplier supplier = null;
            if (!string.IsNullOrEmpty(dependency.ScopeName))
            {
                lock (suppliers) supplier = suppliers.FirstOrDefault(s => !s.IsScoped && s.IsSupplierOf(dependency));
            }

            if (supplier == null)
            {
                lock (suppliers) supplier = suppliers.FirstOrDefault(s => s.IsScoped && s.IsSupplierOf(dependency));
            }

            return supplier;
        }
    }
}
