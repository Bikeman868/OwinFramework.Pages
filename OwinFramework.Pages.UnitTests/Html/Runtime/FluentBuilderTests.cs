using System;
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

namespace OwinFramework.Pages.UnitTests.Html.Runtime
{
    [TestFixture]
    public class FluentBuilderTests: TestBase
    {
        private IDataProviderDependenciesFactory _dataProviderDependenciesFactory;
        private INameManager _nameManager;
        private IRequestRouter _requestRouter;
        private IFluentBuilder _fluentBuilder;

        private IDataCatalog _dataCatalog;

        [SetUp]
        public void Setup()
        {
            Reset();

            _dataProviderDependenciesFactory = SetupMock<IDataProviderDependenciesFactory>();
            _nameManager = SetupMock<INameManager>();
            _requestRouter = SetupMock<IRequestRouter>();
            _dataCatalog = SetupMock<IDataCatalog>();

            _fluentBuilder = new FluentBuilder(_nameManager, _requestRouter, _dataCatalog);
        }

        [Test]
        public void Should_register_data_providers()
        {
            var personProvider = new PersonProvider(SetupMock<IDataProviderDependenciesFactory>());
            _fluentBuilder.Register(personProvider);

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

        public class Person
        {
            public string FirstName;
            public string LastName;
        }

        [IsDataProvider(typeof(Person))]
        public class PersonProvider : DataProvider
        {
            public PersonProvider(IDataProviderDependenciesFactory dependencies)
                : base(dependencies)
            { }

            public override void Supply(
                IRenderContext renderContext,
                IDataContext dataContext,
                IDataDependency dependency)
            {
                dataContext.Set(new Person { FirstName = "Martin", LastName = "Halliday" });
            }
        }
    }
}
