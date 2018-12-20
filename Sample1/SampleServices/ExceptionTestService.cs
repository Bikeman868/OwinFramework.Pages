using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Restful.Interfaces;
using OwinFramework.Pages.Restful.Parameters;

namespace Sample1.SampleServices
{
    [IsService("exceptions", "/exception/", new[] { Method.Post, Method.Get })]
    public class ExceptionTestService
    {
        [Endpoint]
        [Description("Call this endpoint to see what happens when services are not implemented")]
        public void NotImplemented(IEndpointRequest request)
        {
            throw new NotImplementedException("Testing not implemented");
        }

        [Endpoint]
        [Description("Call this endpoint to see what happens when services are not implemented")]
        public void Aggregate(IEndpointRequest request)
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