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
    /// This is the base class that the built-in serializers inherit from. You can either
    /// inherit from this or use it as a MixIn
    /// </summary>
    public class SerializerBase
    {
        public Task Redirect(IOwinContext context, Uri url, HttpStatusCode statusCode)
        {
            string message;
            switch (statusCode)
            {
                case HttpStatusCode.TemporaryRedirect:
                    message = "Content temporarily moved, see Location header";
                    break;
                case HttpStatusCode.MovedPermanently:
                    message = "Content permenantly moved, see Location header";
                    break;
                case HttpStatusCode.Found:
                    message = "Content found, see Location header";
                    break;
                default:
                    message = statusCode.ToString();
                    break;
            }

            var location = url.ToString();

            context.Response.Headers["Location"] = location;
            context.Response.StatusCode = (int)statusCode;
            context.Response.ReasonPhrase = message;

            return context.Response.WriteAsync(string.Empty);
        }

        public void AddHeader(IOwinContext context, string name, string value)
        {
            if (string.Equals(name, "Content-Length", StringComparison.OrdinalIgnoreCase))
                context.Response.ContentLength = long.Parse(value);
            if (string.Equals(name, "Content-Type", StringComparison.OrdinalIgnoreCase))
                context.Response.ContentType = value;
            if (string.Equals(name, "E-Tag", StringComparison.OrdinalIgnoreCase))
                context.Response.ETag = value;
            if (string.Equals(name, "Expires", StringComparison.OrdinalIgnoreCase))
                context.Response.Expires = DateTimeOffset.Parse(value);
            else
                context.Response.Headers[name] = value;
        }

        public void SetCookie(
            IOwinContext context, 
            string name, 
            string value, 
            TimeSpan expiry,
            string domain = null,
            bool secure = false)
        {
            var options = new CookieOptions 
            { 
                Domain = domain, 
                Expires = DateTime.UtcNow + expiry, 
                Secure = secure
            };
            context.Response.Cookies.Append(name, value, options);
        }

        public void DeleteCookie(IOwinContext context, string name, string domain = null)
        {
            var options = new CookieOptions
            {
                Domain = domain,
                Expires = DateTime.UtcNow.AddDays(-1),
            };
            context.Response.Cookies.Append(name, string.Empty, options);
        }
    }
}
