using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Restful.Interfaces;

namespace Sample1.SampleServices
{
    [IsService("arithmetic", "/math/", new[] { Methods.Post, Methods.Get })]
    public class ArithmeticService
    {
        [Endpoint]
        [EndpointParameter("a", typeof(double))]
        [EndpointParameter("b", typeof(double))]
        public void Add(IEndpointRequest request)
        {
            var a = request.Parameter<double>("a");
            var b = request.Parameter<double>("b");
            request.Success(a + b);
        }

        [Endpoint]
        [EndpointParameter("a", typeof(double))]
        [EndpointParameter("b", typeof(double))]
        public void Subtract(IEndpointRequest request)
        {
            var a = request.Parameter<double>("a");
            var b = request.Parameter<double>("b");
            request.Success(a - b);
        }

        [Endpoint]
        [EndpointParameter("a", typeof(double))]
        [EndpointParameter("b", typeof(double))]
        public void Multiply(IEndpointRequest request)
        {
            var a = request.Parameter<double>("a");
            var b = request.Parameter<double>("b");
            request.Success(a * b);
        }

        [Endpoint]
        [EndpointParameter("a", typeof(double))]
        [EndpointParameter("b", typeof(double))]
        public void Divide(IEndpointRequest request)
        {
            var a = request.Parameter<double>("a");
            var b = request.Parameter<double>("b");
            request.Success(a / b);
        }
    }
}