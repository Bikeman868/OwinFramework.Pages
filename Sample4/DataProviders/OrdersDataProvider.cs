using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Framework.DataModel;

namespace Sample4.DataProviders
{
    [IsDataProvider("orders", typeof(IList<OrderViewModel>))]
    public class OrdersDataProvider : DataProvider
    {
        private readonly IList<OrderViewModel> _orders;

        public OrdersDataProvider(IDataProviderDependenciesFactory dependencies) 
            : base(dependencies) 
        {
            _orders = new List<OrderViewModel> 
            { 
                new OrderViewModel
                {
                    OrderDate = DateTime.Parse("2019-05-01"),
                    Sku = "ABC123",
                    Quantity = 16,
                    PriceEach = 1.24m
                },
                new OrderViewModel
                {
                    OrderDate = DateTime.Parse("2019-05-03"),
                    Sku = "DEF987",
                    Quantity = 3,
                    PriceEach = 96.34m
                },
                new OrderViewModel
                {
                    OrderDate = DateTime.Parse("2019-05-05"),
                    Sku = "XYZ654",
                    Quantity = 1,
                    PriceEach = 12.45m
                },
            };
        }

        protected override void Supply(
            IRenderContext renderContext,
            IDataContext dataContext,
            IDataDependency dependency)
        {
            dataContext.Set(_orders);
        }
    }
}