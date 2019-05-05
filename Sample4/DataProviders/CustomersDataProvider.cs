using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Framework.DataModel;

namespace Sample4.DataProviders
{
    [IsDataProvider("customers", typeof(IList<CustomerViewModel>))]
    public class CustomersDataProvider : DataProvider
    {
        private readonly IList<CustomerViewModel> _customers;

        public CustomersDataProvider(IDataProviderDependenciesFactory dependencies) 
            : base(dependencies) 
        {
            _customers = new List<CustomerViewModel> 
            { 
                new CustomerViewModel
                {
                    Name = "Cusotmer 1",
                    CreditLimit = 50000,
                    OrderTotal = 245000
                },
                new CustomerViewModel
                {
                    Name = "Cusotmer 2",
                    CreditLimit = 900000,
                    OrderTotal = 635000
                },
                new CustomerViewModel
                {
                    Name = "Cusotmer 3",
                    CreditLimit = 1500,
                    OrderTotal = 18000
                },
            };
        }

        protected override void Supply(
            IRenderContext renderContext,
            IDataContext dataContext,
            IDataDependency dependency)
        {
            dataContext.Set(_customers);
        }
    }
}