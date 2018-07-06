using System;
using Moq;
using Moq.Modules;
using OwinFramework.Pages.Core.Interfaces.DataModel;

namespace OwinFramework.Pages.Mocks.DataModel
{
    public class MockDataDependencyFactory: ConcreteImplementationProvider<IDataDependencyFactory>, IDataDependencyFactory
    {
        protected override IDataDependencyFactory GetImplementation(IMockProducer mockProducer)
        {
            return this;
        }

        public IDataDependency Create(Type type, string scopeName = null)
        {
            return new DataDependency { DataType = type, ScopeName = scopeName };
        }

        public IDataDependency Create<T>(string scopeName = null)
        {
            return new DataDependency { DataType = typeof(T), ScopeName = scopeName };
        }

        private class DataDependency: IDataDependency
        {
            public Type DataType { get; set; }
            public string ScopeName { get; set; }

            public bool Equals(IDataDependency other)
            {
                if (ReferenceEquals(other, null)) return false;
                if (DataType != other.DataType) return false;
                return string.Equals(ScopeName, other.ScopeName, StringComparison.OrdinalIgnoreCase);
            }
        }


    }
}
