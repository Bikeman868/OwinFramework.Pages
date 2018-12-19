using System;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Capability;
using OwinFramework.Pages.Restful.Interfaces;

namespace OwinFramework.Pages.Restful.Runtime
{
    internal class EndpointParameter
    {
        public string Name { get; set; }
        public EndpointParameterType ParameterType { get; set; }
        public IParameterValidator Validator { get; set; }
        public Func<IEndpointRequest, string, string>[] Functions;
    }
}