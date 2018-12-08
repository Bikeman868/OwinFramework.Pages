using System;
using System.Collections.Generic;
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
            var a = request.Parameter<int>("a");
            var b = request.Parameter<int>("b");
            request.Success(a / b);
        }

        [Endpoint]
        public void NotImplemented(IEndpointRequest request)
        {
            throw new NotImplementedException("Testing not implemented");
        }

        [Endpoint]
        public void AggregateException(IEndpointRequest request)
        {
            var exceptions = new List<Exception>();

            try
            {
                throw new OutOfMemoryException();
            }
            catch (Exception e) { exceptions.Add(e); }

            try
            {
                throw new DivideByZeroException();
            }
            catch (Exception e) { exceptions.Add(e); }

            throw new AggregateException("Testing aggregate exception", exceptions);
        }
    }
}