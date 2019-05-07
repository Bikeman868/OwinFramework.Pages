using System;

namespace Sample4.ViewModels
{
    public class OrderViewModel
    {
        public DateTime OrderDate { get; set; }
        public string Sku { get; set; }
        public int Quantity { get; set; }
        public decimal PriceEach { get; set; }
    }
}