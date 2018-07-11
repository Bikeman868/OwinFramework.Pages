using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Moq.Modules;
using OwinFramework.Pages.Core.Interfaces.DataModel;

namespace OwinFramework.Pages.Mocks.DataModel
{
    public class MockDataCatalog : ConcreteImplementationProvider<IDataCatalog>, IDataCatalog
    {
        public readonly Dictionary<Type, List<IDataSupplier>> Suppliers = new Dictionary<Type, List<IDataSupplier>>();

        protected override IDataCatalog GetImplementation(IMockProducer mockProducer)
        {
            return this;
        }

        public IDataCatalog Register(IDataSupplier dataSupplier)
        {
            if (dataSupplier == null)
                return this;

            if (ReferenceEquals(dataSupplier.SuppliedTypes, null)) return this;

            foreach (var type in dataSupplier.SuppliedTypes)
            {
                if (!Suppliers.ContainsKey(type))
                    Suppliers[type] = new List<IDataSupplier>();

                var suppliers = Suppliers[type];
                suppliers.Add(dataSupplier);
            }

            return this;
        }

        public IDataCatalog Register(Type dataSupplierType, Func<Type, object> factoryFunc)
        {
            return Register(factoryFunc(dataSupplierType) as IDataSupplier);
        }

        public IDataCatalog Register(Assembly assembly, Func<Type, object> factoryFunc)
        {
            return this;
        }

        public IDataSupplier FindSupplier(IDataDependency dependency)
        {
            List<IDataSupplier> suppliers;
            if (!Suppliers.TryGetValue(dependency.DataType, out suppliers))
                return null;

            IDataSupplier supplier;
            if (string.IsNullOrEmpty(dependency.ScopeName))
                supplier = suppliers.FirstOrDefault(s => !s.IsScoped && s.IsSupplierOf(dependency));
            else
                supplier = suppliers.FirstOrDefault(s => s.IsScoped && s.IsSupplierOf(dependency));

            if (supplier == null)
                supplier = suppliers.FirstOrDefault(s => s.IsSupplierOf(dependency));

            return supplier;
        }
    }
}
