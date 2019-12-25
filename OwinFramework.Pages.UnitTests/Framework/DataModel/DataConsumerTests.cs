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

        [Test, Ignore("Getting consumer needs no longer has teh side-effect of adding data scope rules")]
        public void Should_register_type_dependencies()
        {
            var dataScopeProvider = SetupMock<IDataScopeRules>();

            var mockDataScopeProvider = GetMock<MockDataScopeRules, IDataScopeRules>();
            mockDataScopeProvider.SupplyAction = dc => dc.Set(new TestType { Value = 99 });

            _dataConsumer.HasDependency<TestType>();
            var needs = _dataConsumer.GetConsumerNeeds();

            Assert.IsNotNull(needs);
            Assert.IsTrue(mockDataScopeProvider.DataDependencies.Any(d => d.DataType == typeof(TestType)));
        }

        private class TestType
        {
            public int Value;
        }
    }
}
