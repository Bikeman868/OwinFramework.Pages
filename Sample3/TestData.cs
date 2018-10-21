using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Framework.DataModel;

namespace Sample3
{
    internal class ApplicationInfo
    {
        public string Name { get { return "My Application"; } }
    }

    internal class Person
    {
        public string Name { get; set; }
        public Address Address { get; set; }
    }

    internal class Address
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
    }

    [IsDataProvider("application", typeof(ApplicationInfo))]
    internal class ApplicationInfoDataProvider : DataProvider
    {
        private readonly ApplicationInfo _applicationInfo;

        public ApplicationInfoDataProvider(IDataProviderDependenciesFactory dependencies)
            : base(dependencies)
        {
            _applicationInfo = new ApplicationInfo();
        }

        protected override void Supply(
            IRenderContext renderContext,
            IDataContext dataContext,
            IDataDependency dependency)
        {
            dataContext.Set(_applicationInfo);
        }
    }

    [IsDataProvider("people", typeof(IList<Person>))]
    internal class PersonListProvider : DataProvider
    {
        public PersonListProvider(IDataProviderDependenciesFactory dependencies)
            : base(dependencies) { }

        protected override void Supply(IRenderContext renderContext, IDataContext dataContext, IDataDependency dependency)
        {
            var people = new[]
            {
                new Person { Name = "Person 1", Address = new Address { Street = "1 Main St", City = "City", ZipCode = "12345" }},
                new Person { Name = "Person 2", Address = new Address { Street = "2 Main St", City = "City", ZipCode = "54321" }},
                new Person { Name = "Person 3", Address = new Address { Street = "3 Main St", City = "City", ZipCode = "99999" }},
            };
            dataContext.Set<IList<Person>>(people);
        }
    }

    [IsDataProvider("person_address")]
    [SuppliesData(typeof(Address))]
    [NeedsData(typeof(Person))]
    internal class PersonAddressProvider : DataProvider
    {
        public PersonAddressProvider(IDataProviderDependenciesFactory dependencies)
            : base(dependencies) { }

        protected override void Supply(IRenderContext renderContext, IDataContext dataContext, IDataDependency dependency)
        {
            var person = dataContext.Get<Person>();
            dataContext.Set(person.Address);
        }
    }

}