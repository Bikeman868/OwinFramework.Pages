using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq.Modules;
using NUnit.Framework;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Framework.DataModel;
using OwinFramework.Pages.Mocks.Runtime;

namespace OwinFramework.Pages.UnitTests.Framework.DataModel
{
    [TestFixture]
    public class DataConsumerTests: TestBase
    {
        private IDataConsumer _dataConsumer;

        [SetUp]
        public void Setup()
        {
            Reset();

            _dataConsumer = new DataConsumer(
                SetupMock<IDataSupplierFactory>(),
                SetupMock<IDataDependencyFactory>());
        }

        [Test]
        public void Should_register_type_dependencies()
        {
            var dataScopeProvider = SetupMock<IDataScopeProvider>();
            var renderContext = SetupMock<IRenderContext>();
            var dataContext = SetupMock<IDataContext>();

            var mockDataScopeProvider = GetMock<MockDataScopeProvider, IDataScopeProvider>();
            mockDataScopeProvider.SupplyAction = dc => dc.Set(new TestType { Value = 99 });

            _dataConsumer.HasDependency<TestType>();
            var dataSuppliers = _dataConsumer.GetDependencies(dataScopeProvider);

            Assert.IsNotNull(dataSuppliers);
            Assert.AreEqual(1, dataSuppliers.Count);

            var dataSupplier = dataSuppliers[0];

            dataSupplier.Supply(renderContext, dataContext);

            var testType = dataContext.Get<TestType>();
            Assert.IsNotNull(testType);
            Assert.AreEqual(99, testType.Value);
        }

        private class TestType
        {
            public int Value;
        }
    }
}
