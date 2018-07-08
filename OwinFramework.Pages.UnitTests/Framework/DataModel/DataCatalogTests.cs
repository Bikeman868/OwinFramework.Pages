using System;
using System.Collections.Generic;
using Moq.Modules;
using NUnit.Framework;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Framework.DataModel;

namespace OwinFramework.Pages.UnitTests.Framework.DataModel
{
    [TestFixture]
    public class DataCatalogTests: TestBase
    {
        private IDataCatalog _dataCatalog;

        [SetUp]
        public void Setup()
        {
            Reset();

            _dataCatalog = new DataCatalog(
                SetupMock<IDictionaryFactory>());
        }

        [Test]
        public void Should_find_scoped_suppliers()
        {
            const string scopeName = "my-scope";

            var dataSupplier = new DataSupplier<int> { ScopeName = scopeName };
            _dataCatalog.Register(dataSupplier);

            var dataDependencyFactory = SetupMock<IDataDependencyFactory>();
            var scopedDependency = dataDependencyFactory.Create<int>(scopeName);
            var unscopedDependency = dataDependencyFactory.Create<int>();

            var found = _dataCatalog.FindSupplier(scopedDependency);
            Assert.IsTrue(ReferenceEquals(dataSupplier, found));

            found = _dataCatalog.FindSupplier(unscopedDependency);
            Assert.IsTrue(ReferenceEquals(dataSupplier, found));
        }

        [Test]
        public void Should_find_unscoped_suppliers()
        {
            const string scopeName = "my-scope";

            var dataSupplier = new DataSupplier<int> { ScopeName = null };
            _dataCatalog.Register(dataSupplier);

            var dataDependencyFactory = SetupMock<IDataDependencyFactory>();
            var scopedDependency = dataDependencyFactory.Create<int>(scopeName);
            var unscopedDependency = dataDependencyFactory.Create<int>();

            var found = _dataCatalog.FindSupplier(scopedDependency);
            Assert.IsTrue(ReferenceEquals(dataSupplier, found));

            found = _dataCatalog.FindSupplier(unscopedDependency);
            Assert.IsTrue(ReferenceEquals(dataSupplier, found));
        }

        [Test]
        public void Should_prioritize_scoped_suppliers()
        {
            const string scopeName = "my-scope";

            var scopedSupplier = new DataSupplier1<int> { ScopeName = scopeName };
            var unscopedSupplier = new DataSupplier2<int> { ScopeName = null };
            _dataCatalog.Register(scopedSupplier);
            _dataCatalog.Register(unscopedSupplier);

            var dataDependencyFactory = SetupMock<IDataDependencyFactory>();
            var scopedDependency = dataDependencyFactory.Create<int>(scopeName);
            var unscopedDependency = dataDependencyFactory.Create<int>();

            var found = _dataCatalog.FindSupplier(scopedDependency);
            Assert.IsTrue(ReferenceEquals(scopedSupplier, found));

            found = _dataCatalog.FindSupplier(unscopedDependency);
            Assert.IsTrue(ReferenceEquals(unscopedSupplier, found));
        }

        private class DataSupplier<T>: IDataSupplier
        {
            public bool IsScoped { get { return !string.IsNullOrEmpty(ScopeName); } }
            public string ScopeName { get; set; }

            public IList<Type> SuppliedTypes
            {
                get { return new List<Type> { typeof(T) }; }
            }

            public void Add(IDataDependency dependency, Action<IRenderContext, IDataContext, IDataDependency> action)
            {
                throw new NotImplementedException();
            }

            public bool IsSupplierOf(IDataDependency dependency)
            {
                if (dependency == null) return false;
                if (dependency.DataType != typeof(T)) return false;
                if (!IsScoped || string.IsNullOrEmpty(dependency.ScopeName)) return true;
                return string.Equals(dependency.ScopeName, ScopeName, StringComparison.OrdinalIgnoreCase);
            }

            public IDataSupply GetSupply(IDataDependency dependency)
            {
                throw new NotImplementedException();
            }
        }

        private class DataSupplier1<T> : DataSupplier<T> { }
        private class DataSupplier2<T> : DataSupplier<T> { }

    }
}
