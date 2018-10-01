using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq.Modules;
using NUnit.Framework;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
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
                SetupMock<IDataDependencyFactory>(),
                SetupMock<IIdManager>());
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
        public void Should_resolve_unscoped_requests_to_scoped_types()
        {
            var renderContext = SetupMock<IRenderContext>();
            var dataContextBuilder = SetupMock<IDataContextBuilder>();

            var mockDataContextBuilder = GetMock<MockDataContextBuilder, IDataContextBuilder>();
            mockDataContextBuilder.DependenciesInScope.Add(new DataDependency { DataType = typeof(TestDto), ScopeName = "scopeName" });

            using (var dataContext = _dataContextFactory.Create(renderContext, dataContextBuilder))
            {
                dataContext.Set(new TestDto { Id = 1, Name = "Martin" }, "scopeName");
                var result = dataContext.Get<TestDto>();

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Id);
                Assert.AreEqual("Martin", result.Name);
            }
        }

        [Test]
        public void Should_not_resolve_scoped_requests_to_unscoped_types()
        {
            var renderContext = SetupMock<IRenderContext>();
            var dataContextBuilder = SetupMock<IDataContextBuilder>();

            var mockDataContextBuilder = GetMock<MockDataContextBuilder, IDataContextBuilder>();
            mockDataContextBuilder.DependenciesInScope.Add(new DataDependency { DataType = typeof(TestDto), ScopeName = "scopeName" });

            using (var dataContext = _dataContextFactory.Create(renderContext, dataContextBuilder))
            {
                dataContext.Set(new TestDto { Id = 1, Name = "Martin" });

                try
                {
                    dataContext.Get<TestDto>("scopeName");
                    Assert.Fail("Should throw an exception when data is requested for scope");
                }
                catch
                { }
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

        [Test]
        public void Should_resolve_using_scoped_types()
        {
            var renderContext = SetupMock<IRenderContext>();
            var dataContextBuilder = SetupMock<IDataContextBuilder>();

            var mockDataContextBuilder = GetMock<MockDataContextBuilder, IDataContextBuilder>();
            mockDataContextBuilder.DependenciesInScope.Add(new DataDependency { DataType = typeof(TestDto), ScopeName = "scope1" });
            mockDataContextBuilder.DependenciesInScope.Add(new DataDependency { DataType = typeof(TestDto), ScopeName = "scope2" });

            using (var dataContext = _dataContextFactory.Create(renderContext, dataContextBuilder))
            {
                dataContext.Set(new TestDto { Id = 1, Name = "Martin" });
                dataContext.Set(new TestDto { Id = 2, Name = "Martin" }, "scope1");
                dataContext.Set(new TestDto { Id = 3, Name = "Martin" }, "scope2");

                var result1 = dataContext.Get<TestDto>();

                Assert.IsNotNull(result1);
                Assert.AreEqual(1, result1.Id);
                Assert.AreEqual("Martin", result1.Name);

                var result2 = dataContext.Get<TestDto>("scope1");

                Assert.IsNotNull(result2);
                Assert.AreEqual(2, result2.Id);
                Assert.AreEqual("Martin", result2.Name);

                var result3 = dataContext.Get<TestDto>("scope2");

                Assert.IsNotNull(result3);
                Assert.AreEqual(3, result3.Id);
                Assert.AreEqual("Martin", result3.Name);
            }
        }

        [Test]
        public void Context_root_should_be_global_scope()
        {
            var renderContext = SetupMock<IRenderContext>();
            var dataContextBuilder = SetupMock<IDataContextBuilder>();

            using (var dataContext = _dataContextFactory.Create(renderContext, dataContextBuilder))
            {
                dataContext.Set(new TestDto { Id = 1, Name = "Martin" });
                dataContext.Set(new TestDto { Id = 2, Name = "Martin" }, "scope1");
                dataContext.Set(new TestDto { Id = 3, Name = "Martin" }, "scope2");

                var result1 = dataContext.Get<TestDto>();

                Assert.IsNotNull(result1);
                Assert.AreEqual(1, result1.Id);
                Assert.AreEqual("Martin", result1.Name);

                var result2 = dataContext.Get<TestDto>("scope1");

                Assert.IsNotNull(result2);
                Assert.AreEqual(2, result2.Id);
                Assert.AreEqual("Martin", result2.Name);

                var result3 = dataContext.Get<TestDto>("scope2");

                Assert.IsNotNull(result3);
                Assert.AreEqual(3, result3.Id);
                Assert.AreEqual("Martin", result3.Name);
            }
        }

        [Test]
        public void Should_defer_to_parent_scope()
        {
            var renderContext = SetupMock<IRenderContext>();

            var queueFactory = SetupMock<IQueueFactory>();
            var dictionaryFactory = SetupMock<IDictionaryFactory>();
            var dataDependencyFactory = SetupMock<IDataDependencyFactory>();
            var idManager = SetupMock<IIdManager>();
            var dataContextFactory = new DataContextFactory(queueFactory, dictionaryFactory, dataDependencyFactory, idManager);

            var dataCatalog = SetupMock<IDataCatalog>();
            var dataContextBuilderFactory = new DataContextBuilderFactory(dataContextFactory, idManager, dataCatalog);

            var dataScopeFactory = new DataScopeFactory();
            var rootScopeRules = new DataScopeRules(dataScopeFactory);
            var childScopeRules = new DataScopeRules(dataScopeFactory);

            rootScopeRules.AddScope(typeof(TestDto), "scopeName");

            var rootDataContextBuilder = dataContextBuilderFactory.Create(rootScopeRules);
            var childDataContextBuilder = rootDataContextBuilder.AddChild(childScopeRules);

            using (var parentContext = _dataContextFactory.Create(renderContext, childDataContextBuilder))
            {
                using (var childContext = parentContext.CreateChild(childDataContextBuilder))
                {
                    childContext.Set(new TestDto { Id = 1, Name = "Martin" }, "scopeName");
                    
                    var result1 = parentContext.Get<TestDto>();

                    Assert.IsNotNull(result1);
                    Assert.AreEqual(1, result1.Id);
                    Assert.AreEqual("Martin", result1.Name);

                    var result2 = parentContext.Get<TestDto>("scopeName");

                    Assert.IsNotNull(result2);
                    Assert.AreEqual(1, result2.Id);
                    Assert.AreEqual("Martin", result2.Name);
                }
            }
        }

        private class TestDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
