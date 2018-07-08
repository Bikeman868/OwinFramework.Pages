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
                SetupMock<IDataDependencyFactory>());
        }

        [Test]
        public void Should_register_type_dependencies()
        {
            var dataScopeProvider = SetupMock<IDataScopeProvider>();

            var mockDataScopeProvider = GetMock<MockDataScopeProvider, IDataScopeProvider>();
            mockDataScopeProvider.SupplyAction = dc => dc.Set(new TestType { Value = 99 });

            _dataConsumer.HasDependency<TestType>();
            _dataConsumer.AddDependenciesToScopeProvider(dataScopeProvider);

            Assert.IsTrue(mockDataScopeProvider.DataDependencies.Any(d => d.DataType == typeof(TestType)));
        }

        private class TestType
        {
            public int Value;
        }
    }
}
