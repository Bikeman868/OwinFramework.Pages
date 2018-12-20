using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Restful.Interfaces;
using OwinFramework.Pages.Restful.Parameters;

namespace Sample1.SampleServices
{
    [IsService("arithmetic", "/math/", new[] { Method.Post, Method.Get })]
    public class ArithmeticService
    {
        [Endpoint(UrlPath = "add/{a}/{b}")]
        [EndpointParameter("a", typeof(double), EndpointParameterType.PathSegment)]
        [EndpointParameter("b", typeof(double), EndpointParameterType.PathSegment)]
        public void Add1(IEndpointRequest request)
        {
            var a = request.Parameter<double>("a");
            var b = request.Parameter<double>("b");
            request.Success(a + b);
        }

        [Endpoint(UrlPath = "add")]
        [EndpointParameter("a", typeof(AnyValue<double>), EndpointParameterType.FormField | EndpointParameterType.QueryString)]
        [EndpointParameter("b", typeof(AnyValue<double>))]
        [Description("Adds two numbers and returns the sum")]
        public void Add2(IEndpointRequest request)
        {
            var a = request.Parameter<double>("a");
            var b = request.Parameter<double>("b");
            request.Success(a + b);
        }

        [Endpoint]
        [EndpointParameter("a", typeof(AnyValue<double>))]
        [EndpointParameter("b", typeof(AnyValue<double>))]
        [Description("Calculates a-b and returns the result")]
        [Example("http://myservice.com/subtract?a=12&b=6")]
        public void Subtract(IEndpointRequest request)
        {
            var a = request.Parameter<double>("a");
            var b = request.Parameter<double>("b");
            request.Success(a - b);
        }

        [Endpoint]
        [EndpointParameter("a", typeof(AnyValue<double>))]
        [EndpointParameter("b", typeof(AnyValue<double>))]
        public void Multiply(IEndpointRequest request)
        {
            var a = request.Parameter<double>("a");
            var b = request.Parameter<double>("b");
            request.Success(a * b);
        }

        [Endpoint]
        [EndpointParameter("a", typeof(AnyValue<double>))]
        [EndpointParameter("b", typeof(NonZeroValue<double>))]
        public void Divide(IEndpointRequest request)
        {
            var a = request.Parameter<double>("a");
            var b = request.Parameter<double>("b");
            request.Success(a / b);
        }
    }
}