using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Restful.Interfaces;

namespace OwinFramework.Pages.Restful.Runtime
{
    internal class EndpointRequest: IEndpointRequest, IDisposable
    {
        private readonly IOwinContext _context;

        public EndpointRequest(IOwinContext context)
        {
            _context = context;
        }

        public void Dispose()
        { }

        public Task WriteResponse()
        {
            return _context.Response.WriteAsync("TBD");
        }

        public IOwinContext OwinContext
        {
            get { return _context; }
        }

        public T Parameter<T>(string name)
        {
            return default(T);
        }

        public void Success()
        {
            throw new NotImplementedException();
        }

        public void Success<T>(T valueToSerialize)
        {
            throw new NotImplementedException();
        }

        public void HttpStatus(HttpStatusCode statusCode, string message)
        {
            _context.Response.StatusCode = (int)statusCode;
            _context.Response.ReasonPhrase = message;
        }

        public void NotFound(string message)
        {
            HttpStatus(HttpStatusCode.NotFound, message ?? "Not Found");
        }

        public void NoContent(string message)
        {
            HttpStatus(HttpStatusCode.NoContent, message ?? "No Content");
        }

        public void BadRequest(string message)
        {
            HttpStatus(HttpStatusCode.BadRequest, message ?? "Bad Request");
        }

        public void Redirect(Uri url, bool permenant = false)
        {
            throw new NotImplementedException();
        }

        public void Rewrite(Uri url, Methods httpMethod = Methods.Get)
        {
            throw new NotImplementedException();
        }
    }
}