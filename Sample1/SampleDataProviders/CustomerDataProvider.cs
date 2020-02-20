using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Framework.DataModel;

namespace Sample1.SampleDataProviders
{
    internal class Customer
    {
        public long Id { get { return 12345678; } }
        public string Name { get { return "A good customer"; } }
    }

    internal class Order
    {
        public long CustomerId { get; set; }
        public string Name { get { return "Widget"; } }
        public int Quantity { get { return 3; } }
    }

    [IsDataProvider("customer", typeof(Customer))]
    internal class CustomerDataProvider : DataProvider
    {
        public CustomerDataProvider(IDataProviderDependenciesFactory dependencies) 
            : base(dependencies)
        {
        }

        protected override void Supply(
            IRenderContext renderContext,
            IDataContext dataContext,
            IDataDependency dependency)
        {
            dataContext.Set(new Customer());
        }
    }

    [IsDataProvider("orders", typeof(IList<Order>))]
    [NeedsData(typeof(Customer))]
    internal class OrderListDataProvider : DataProvider
    {
        public OrderListDataProvider(IDataProviderDependenciesFactory dependencies) 
            : base(dependencies)
        {
        }

        protected override void Supply(
            IRenderContext renderContext,
            IDataContext dataContext,
            IDataDependency dependency)
        {
            var customer = dataContext.Get<Customer>();
            var orders = new List<Order>
            {
                new Order { CustomerId = customer.Id }
            };

            dataContext.Set<IList<Order>>(orders);
        }
    }
}