using System;
using System.Reflection;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Restful.Interfaces;
using OwinFramework.Pages.Restful.Runtime;
using OwinFramework.Pages.Restful.Serializers;

namespace Sample1.SampleServices
{
    [IsService("hello_service")]
    public class HelloService: Service
    {
        private readonly string _helloMessage;

        public HelloService(IServiceDependenciesFactory serviceDependenciesFactory) 
            : base(serviceDependenciesFactory)
        {
            _helloMessage =
                "Hello from version " + Assembly.GetExecutingAssembly().GetName().Version + 
                " running on " + Environment.MachineName;
        }

        [Endpoint(UrlPath = "/hello", Methods = new []{ Method.Get }, ResponseSerializer = typeof(PlainText))]
        [Description("This endpoint can be pinged as a health check")]
        private void Hello(IEndpointRequest request)
        {
            request.Success(_helloMessage);
        }
    }
}
