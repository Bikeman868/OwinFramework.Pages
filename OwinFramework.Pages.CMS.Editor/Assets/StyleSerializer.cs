using System.Net;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Pages.Restful.Interfaces;
using OwinFramework.Pages.Restful.Serializers;

namespace OwinFramework.Pages.CMS.Editor.Assets
{
    /// <summary>
    /// This serializes CSS
    /// </summary>
    public class StyleSerializer : SerializerBase, IResponseSerializer
    {
        public Task Success(IOwinContext context)
        {
            context.Response.ContentType =  "text/css";
            return context.Response.WriteAsync(string.Empty);
        }

        public Task Success<T>(IOwinContext context, T data)
        {
            context.Response.ContentType =  "text/css";
            return context.Response.WriteAsync(data.ToString());
        }

        public Task HttpStatus(IOwinContext context, HttpStatusCode statusCode, string message)
        {
            message = message ?? statusCode.ToString();

            context.Response.StatusCode = (int)statusCode;
            context.Response.ReasonPhrase = message;
            context.Response.ContentType =  "text/css";
            return context.Response.WriteAsync(message);
        }
    }
}
