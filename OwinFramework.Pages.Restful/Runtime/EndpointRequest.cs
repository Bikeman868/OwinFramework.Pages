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
        private readonly IRequestDeserializer _deserializer;
        private readonly IResponseSerializer _serializer;

        private Func<Task> _writeResponse;

        public EndpointRequest(
            IOwinContext context,
            IRequestDeserializer deserializer,
            IResponseSerializer serializer)
        {
            _context = context;
            _deserializer = deserializer;
            _serializer = serializer;

            Success();
        }

        public void Dispose()
        { }

        public Task WriteResponse()
        {
            return _writeResponse();
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
            _writeResponse = () => _serializer.Success(_context);
        }

        public void Success<T>(T valueToSerialize)
        {
            _writeResponse = () => _serializer.Success(_context, valueToSerialize);
        }

        public void HttpStatus(HttpStatusCode statusCode, string message)
        {
            _writeResponse = () => _serializer.HttpStatus(_context, statusCode, message);
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
            _writeResponse = () => _serializer.Redirect(_context, url, permenant);
        }

        public void Rewrite(Uri url, Methods httpMethod = Methods.Get)
        {
            throw new NotImplementedException();
        }
    }
}