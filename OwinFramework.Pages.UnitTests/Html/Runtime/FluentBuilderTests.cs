﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq.Modules;
using NUnit.Framework;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Framework.Builders;
using OwinFramework.Pages.Framework.DataModel;
using OwinFramework.Pages.Framework.Interfaces;

namespace OwinFramework.Pages.UnitTests.Html.Runtime
{
    [TestFixture]
    public class FluentBuilderTests: TestBase
    {
        private IDataProviderDependenciesFactory _dataProviderDependenciesFactory;
        private INameManager _nameManager;
        private IFluentBuilder _fluentBuilder;
        private IElementConfiguror _elementConfiguror;
        private IDataSupplierFactory _dataSupplierFactory;
        private IDataDependencyFactory _dataDependencyFactory;
        private IDataCatalog _dataCatalog;
        private IRequestRouter _requestRouter;
        private IFrameworkConfiguration _frameworkConfiguration;

        [SetUp]
        public void Setup()
        {
            Reset();

            _dataProviderDependenciesFactory = SetupMock<IDataProviderDependenciesFactory>();
            _nameManager = SetupMock<INameManager>();
            _dataCatalog = SetupMock<IDataCatalog>();
            _dataSupplierFactory = SetupMock<IDataSupplierFactory>();
            _dataDependencyFactory = SetupMock<IDataDependencyFactory>();
            _requestRouter = SetupMock<IRequestRouter>();
            _frameworkConfiguration = SetupMock<IFrameworkConfiguration>();

            _elementConfiguror = new ElementConfiguror(
                _dataSupplierFactory,
                _dataDependencyFactory,
                _nameManager,
                _requestRouter);

            _fluentBuilder = new FluentBuilder(
                _nameManager,
                _dataCatalog,
                _dataProviderDependenciesFactory.DataDependencyFactory,
                _dataProviderDependenciesFactory.DataSupplierFactory,
                _frameworkConfiguration);

            _fluentBuilder.DataProviderBuilder = new DataProviderBuilder(
                _dataProviderDependenciesFactory,
                _elementConfiguror,
                _nameManager,
                _fluentBuilder);
        }

        [Test]
        public void Should_register_unscoped_data_provider_and_resolve_unscoped()
        {
            var personProvider = new PersonProvider1(_dataProviderDependenciesFactory);
            var builtDataProvider = _fluentBuilder.BuildUpDataProvider(personProvider).Build();

            Assert.IsNotNull(builtDataProvider);

            var dataProvider = _nameManager.ResolveDataProvider(personProvider.Name);
            Assert.IsTrue(ReferenceEquals(dataProvider, personProvider));

            var dependency = _dataProviderDependenciesFactory.DataDependencyFactory.Create<Person>();
            var dataSupplier = _dataCatalog.FindSupplier(dependency);

            Assert.IsNotNull(dataSupplier);

            var dataSupply = dataSupplier.GetSupply(dependency);

            Assert.IsNotNull(dataSupply);

            var renderContext = SetupMock<IRenderContext>();
            var dataContext = SetupMock<IDataContext>();
            dataSupply.Supply(renderContext, dataContext);

            var person = dataContext.Get<Person>();

            Assert.IsNotNull(person);
            Assert.AreEqual("Martin", person.FirstName);
            Assert.AreEqual("Halliday", person.LastName);
        }

        [Test]
        public void Should_register_unscoped_data_provider_and_resolve_scoped()
        {
            var personProvider = new PersonProvider1(_dataProviderDependenciesFactory);
            var builtDataProvider = _fluentBuilder.BuildUpDataProvider(personProvider).Build();

            Assert.IsNotNull(builtDataProvider);

            var dataProvider = _nameManager.ResolveDataProvider(personProvider.Name);
            Assert.IsTrue(ReferenceEquals(dataProvider, personProvider));

            var dependency = _dataProviderDependenciesFactory.DataDependencyFactory.Create<Person>("scope");
            var dataSupplier = _dataCatalog.FindSupplier(dependency);

            Assert.IsNotNull(dataSupplier);

            var dataSupply = dataSupplier.GetSupply(dependency);

            Assert.IsNotNull(dataSupply);

            var renderContext = SetupMock<IRenderContext>();
            var dataContext = SetupMock<IDataContext>();
            dataSupply.Supply(renderContext, dataContext);

            var person = dataContext.Get<Person>();

            Assert.IsNotNull(person);
            Assert.AreEqual("Martin", person.FirstName);
            Assert.AreEqual("Halliday", person.LastName);
        }

        [Test]
        public void Should_register_scoped_data_provider_and_resolve_unscoped()
        {
            var personProvider = new PersonProvider2(_dataProviderDependenciesFactory);
            var builtDataProvider = _fluentBuilder.BuildUpDataProvider(personProvider).Build();

            Assert.IsNotNull(builtDataProvider);

            var dataProvider = _nameManager.ResolveDataProvider(personProvider.Name);
            Assert.IsTrue(ReferenceEquals(dataProvider, personProvider));

            var dependency = _dataProviderDependenciesFactory.DataDependencyFactory.Create<Person>();
            var dataSupplier = _dataCatalog.FindSupplier(dependency);

            Assert.IsNotNull(dataSupplier);

            var dataSupply = dataSupplier.GetSupply(dependency);

            Assert.IsNotNull(dataSupply);

            var renderContext = SetupMock<IRenderContext>();
            var dataContext = SetupMock<IDataContext>();
            dataSupply.Supply(renderContext, dataContext);

            var person = dataContext.Get<Person>();

            Assert.IsNotNull(person);
            Assert.AreEqual("John", person.FirstName);
            Assert.AreEqual("Doe", person.LastName);
        }

        [Test]
        public void Should_register_scoped_data_provider_and_resolve_scoped()
        {
            var personProvider = new PersonProvider2(_dataProviderDependenciesFactory);
            var builtDataProvider = _fluentBuilder.BuildUpDataProvider(personProvider).Build();

            Assert.IsNotNull(builtDataProvider);

            var dataProvider = _nameManager.ResolveDataProvider(personProvider.Name);
            Assert.IsTrue(ReferenceEquals(dataProvider, personProvider));

            var dependency = _dataProviderDependenciesFactory.DataDependencyFactory.Create<Person>("logged-in");
            var dataSupplier = _dataCatalog.FindSupplier(dependency);

            Assert.IsNotNull(dataSupplier);

            var dataSupply = dataSupplier.GetSupply(dependency);

            Assert.IsNotNull(dataSupply);

            var renderContext = SetupMock<IRenderContext>();
            var dataContext = SetupMock<IDataContext>();
            dataSupply.Supply(renderContext, dataContext);

            var person = dataContext.Get<Person>();

            Assert.IsNotNull(person);
            Assert.AreEqual("John", person.FirstName);
            Assert.AreEqual("Doe", person.LastName);
        }

        [Test]
        public void Should_distinguish_scoped_data_provider()
        {
            var personProvider1 = new PersonProvider1(_dataProviderDependenciesFactory);
            _fluentBuilder.BuildUpDataProvider(personProvider1).Build();

            var personProvider2 = new PersonProvider2(_dataProviderDependenciesFactory);
            _fluentBuilder.BuildUpDataProvider(personProvider2).Build();

            var dependency = _dataProviderDependenciesFactory.DataDependencyFactory.Create<Person>("logged-in");
            var dataSupplier = _dataCatalog.FindSupplier(dependency);

            var dataSupply = dataSupplier.GetSupply(dependency);

            Assert.IsNotNull(dataSupply);

            var renderContext = SetupMock<IRenderContext>();
            var dataContext = SetupMock<IDataContext>();
            dataSupply.Supply(renderContext, dataContext);

            var person = dataContext.Get<Person>();

            Assert.IsNotNull(person);
            Assert.AreEqual("John", person.FirstName);
            Assert.AreEqual("Doe", person.LastName);
        }

        [Test]
        public void Should_distinguish_unscoped_data_provider()
        {
            var personProvider1 = new PersonProvider1(_dataProviderDependenciesFactory);
            _fluentBuilder.BuildUpDataProvider(personProvider1).Build();

            var personProvider2 = new PersonProvider2(_dataProviderDependenciesFactory);
            _fluentBuilder.BuildUpDataProvider(personProvider2).Build();

            var dependency1 = _dataProviderDependenciesFactory.DataDependencyFactory.Create<Person>();
            var dataSupplier1 = _dataCatalog.FindSupplier(dependency1);

            var dataSupply = dataSupplier1.GetSupply(dependency1);

            Assert.IsNotNull(dataSupply);

            var renderContext = SetupMock<IRenderContext>();
            var dataContext = SetupMock<IDataContext>();
            dataSupply.Supply(renderContext, dataContext);

            var person = dataContext.Get<Person>();

            Assert.IsNotNull(person);
            Assert.AreEqual("Martin", person.FirstName);
            Assert.AreEqual("Halliday", person.LastName);
        }

        public class Person
        {
            public string FirstName;
            public string LastName;
        }

        [IsDataProvider(typeof(Person))]
        public class PersonProvider1 : DataProvider
        {
            public PersonProvider1(IDataProviderDependenciesFactory dependencies)
                : base(dependencies)
            { }

            protected override void Supply(
                IRenderContext renderContext,
                IDataContext dataContext,
                IDataDependency dependency)
            {
                dataContext.Set(new Person { FirstName = "Martin", LastName = "Halliday" });
            }
        }

        [IsDataProvider(typeof(Person), "logged-in")]
        public class PersonProvider2 : DataProvider
        {
            public PersonProvider2(IDataProviderDependenciesFactory dependencies)
                : base(dependencies)
            { }

            protected override void Supply(
                IRenderContext renderContext,
                IDataContext dataContext,
                IDataDependency dependency)
            {
                dataContext.Set(new Person { FirstName = "John", LastName = "Doe" });
            }
        }
    }
}
