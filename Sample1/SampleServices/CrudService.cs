using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Restful.Interfaces;
using OwinFramework.Pages.Restful.Parameters;

namespace Sample1.SampleServices
{
    [IsService("crud", "/thing/", new[] { Method.Post, Method.Get, Method.Put, Method.Delete })]
    public class CrudService
    {
        [Endpoint(UrlPath = "{id}", Methods = new[] { Method.Get })]
        public void Get(IEndpointRequest request)
        {
            throw new NotImplementedException();
        }

        [Endpoint(UrlPath = "{id}", Methods = new[] { Method.Post })]
        public void Post(IEndpointRequest request)
        {
            throw new NotImplementedException();
        }

        [Endpoint(UrlPath = "{id}", Methods = new[] { Method.Put })]
        public void Put(IEndpointRequest request)
        {
            throw new NotImplementedException();
        }

        [Endpoint(UrlPath = "{id}", Methods = new[] { Method.Delete })]
        public void Delete(IEndpointRequest request)
        {
            throw new NotImplementedException();
        }
    }
}