using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq.Modules;
using NUnit.Framework;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Framework.DataModel;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Mocks.DataModel;

namespace OwinFramework.Pages.UnitTests.Framework.DataModel
{
    [TestFixture]
    public class DataContextFactoryTests: TestBase
    {
        private IDataContextFactory _dataContextFactory;

        [SetUp]
        public void Setup()
        {
            Reset();

            _dataContextFactory = new DataContextFactory(
                SetupMock<IQueueFactory>(),
                SetupMock<IDictionaryFactory>(),
                SetupMock<IDataDependencyFactory>());
        }

        [Test]
        public void Should_persist_unscoped_types()
        {
            var renderContext = SetupMock<IRenderContext>();
            var dataContextBuilder = SetupMock<IDataContextBuilder>();
            using (var dataContext = _dataContextFactory.Create(renderContext, dataContextBuilder))
            {
                dataContext.Set(new TestDto { Id = 1, Name = "Martin" });
                var result = dataContext.Get<TestDto>();
                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Id);
                Assert.AreEqual("Martin", result.Name);
            }
        }

        [Test]
        public void Should_persist_scoped_types()
        {
            var renderContext = SetupMock<IRenderContext>();
            var dataContextBuilder = SetupMock<IDataContextBuilder>();

            var mockDataContextBuilder = GetMock<MockDataContextBuilder, IDataContextBuilder>();
            mockDataContextBuilder.DependenciesInScope.Add(new DataDependency { DataType = typeof(TestDto), ScopeName="scopeName" });

            using (var dataContext = _dataContextFactory.Create(renderContext, dataContextBuilder))
            {
                dataContext.Set(new TestDto { Id = 1, Name = "Martin" }, "scopeName");
                var result = dataContext.Get<TestDto>("scopeName");

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Id);
                Assert.AreEqual("Martin", result.Name);

                result = dataContext.Get<TestDto>("wrongScope", false);
                Assert.IsNull(result);
            }
        }

        [Test]
        public void Should_not_persist_out_of_scope_data()
        {
            var renderContext = SetupMock<IRenderContext>();
            var dataContextBuilder = SetupMock<IDataContextBuilder>();

            using (var dataContext = _dataContextFactory.Create(renderContext, dataContextBuilder))
            {
                dataContext.Set(new TestDto { Id = 1, Name = "Martin" }, "scopeName");

                try
                {
                    dataContext.Get<TestDto>("scopeName");
                    Assert.Fail("Should throw an exception when data is requested for undefined scope");
                }
                catch
                { }
            }
        }

        private class TestDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
