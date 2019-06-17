using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Pages.Restful.Interfaces;

namespace OwinFramework.Pages.Restful.Serializers
{
    /// <summary>
    /// This serializes strings to html format
    /// </summary>
    public class Html : SerializerBase, IResponseSerializer, IRequestDeserializer
    {
        public T Body<T>(IOwinContext context)
        {
            if (!(typeof(T) is string))
                throw new NotImplementedException("The html request deserializer can only return the request body as a string");

            using (var reader = new StreamReader(context.Request.Body))
            {
                return (T)(object)reader.ReadToEnd();
            }
        }

        public Task Success(IOwinContext context)
        {
            context.Response.ContentType = "text/html";
            return context.Response.WriteAsync("");
        }

        public Task Success<T>(IOwinContext context, T data)
        {
            context.Response.ContentType = "text/html";
            return context.Response.WriteAsync(data.ToString());
        }

        public Task HttpStatus(IOwinContext context, HttpStatusCode statusCode, string message)
        {
            message = message ?? statusCode.ToString();

            context.Response.StatusCode = (int)statusCode;
            context.Response.ReasonPhrase = message;
            context.Response.ContentType = "text/html";
            return context.Response.WriteAsync(message);
        }

    }
}
