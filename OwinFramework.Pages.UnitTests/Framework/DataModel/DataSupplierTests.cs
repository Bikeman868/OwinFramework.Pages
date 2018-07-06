using System;
using Moq.Modules;
using NUnit.Framework;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Framework.DataModel;

namespace OwinFramework.Pages.UnitTests.Framework.DataModel
{
    [TestFixture]
    public class DataSupplierTests: TestBase
    {
        private IDataSupplier _dataSupplier;

        [SetUp]
        public void Setup()
        {
            Reset();

            _dataSupplier = new DataSupplier(
                SetupMock<IIdManager>());
        }

        [Test]
        public void Should_register_supplies_with_scope()
        {
            var dependencyFactory = SetupMock<IDataDependencyFactory>();

            IDataDependency actionDependency = null;
            Action<IRenderContext, IDataContext, IDataDependency> action =
                (rc, dc, dd) => actionDependency = dd;

            var registeredDependency = dependencyFactory.Create(typeof(int), "test-scope");
            _dataSupplier.Add(registeredDependency, action);

            Assert.IsTrue(_dataSupplier.IsSupplierOf(registeredDependency));
            Assert.IsTrue(_dataSupplier.IsSupplierOf(dependencyFactory.Create(typeof(int), "test-scope")));
            Assert.IsTrue(_dataSupplier.IsSupplierOf(dependencyFactory.Create(typeof(int))));

            Assert.IsFalse(_dataSupplier.IsSupplierOf(dependencyFactory.Create(typeof(long))));
            Assert.IsFalse(_dataSupplier.IsSupplierOf(dependencyFactory.Create(typeof(int), "other-scope")));
            Assert.IsFalse(_dataSupplier.IsSupplierOf(dependencyFactory.Create(typeof(long), "test-scope")));
            Assert.IsFalse(_dataSupplier.IsSupplierOf(dependencyFactory.Create(typeof(long), "other-scope")));

            var dataSupply = _dataSupplier.GetSupply(dependencyFactory.Create(typeof(int)));
            dataSupply.Supply(null, null);

            Assert.AreEqual(registeredDependency, actionDependency);
        }

    }
}
