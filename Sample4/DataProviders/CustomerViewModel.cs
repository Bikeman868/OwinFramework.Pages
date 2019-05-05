using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sample4.DataProviders
{
    public class CustomerViewModel
    {
        public string Name { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal OrderTotal { get; set; }
    }
}