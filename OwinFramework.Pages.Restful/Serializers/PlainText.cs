using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Restful.Interfaces;

namespace OwinFramework.Pages.Restful.Serializers
{
    /// <summary>
    /// This serializes to plain text format. This is mostly useful for debugging
    /// </summary>
    public class PlainText : SerializerBase, IResponseSerializer, IRequestDeserializer
    {
        public T Body<T>(IOwinContext context)
        {
            if (!(typeof(T) is string))
                throw new NotImplementedException("The plain text request deserializer can only return the request body as a string");

            using (var reader = new StreamReader(context.Request.Body))
            {
                return (T)(object)reader.ReadToEnd();
            }
        }

        public Task Success(IOwinContext context)
        {
            context.Response.ContentType = "text/plain";
            return context.Response.WriteAsync("Success");
        }

        public Task Success<T>(IOwinContext context, T data)
        {
            context.Response.ContentType = "text/plain";
            if (context.Request.Method == "HEAD")
            {
                context.Response.ContentLength = data.ToString().Length;
                return context.Response.WriteAsync(string.Empty);
            }
            return context.Response.WriteAsync(data.ToString());
        }

        public Task HttpStatus(IOwinContext context, HttpStatusCode statusCode, string message)
        {
            message = message ?? statusCode.ToString();

            context.Response.StatusCode = (int)statusCode;
            context.Response.ReasonPhrase = message;
            context.Response.ContentType = "text/plain";
            return context.Response.WriteAsync(message ?? string.Empty);
        }
    }
}
