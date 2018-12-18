using System;
using System.Collections;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OwinFramework.Pages.Restful.Interfaces;

namespace OwinFramework.Pages.Restful.Serializers
{
    /// <summary>
    /// This is the default serializer, it send the response in Json format
    /// </summary>
    public class Json: IRequestDeserializer, IResponseSerializer
    {
        public T Body<T>(IOwinContext context)
        {
            throw new NotImplementedException();
        }

        public Task Success(IOwinContext context)
        {
            return context.Response.WriteAsync("{\"success\":true}");
        }

        public Task Success<T>(IOwinContext context, T data)
        {
            var type = typeof(T);

            string json;
            if (type.IsValueType || (type == typeof(string) || (type == typeof(DateTime))))
            {
                var value = new JValue(data);
                json = value.ToString();
            }
            else if (type.IsArray || typeof(IEnumerable).IsAssignableFrom(type))
            {
                var value = new JArray(data);
                json = value.ToString();
            }
            else
            {
                json = JsonConvert.SerializeObject(data);
            }
            return context.Response.WriteAsync(json);
        }

        public Task HttpStatus(IOwinContext context, HttpStatusCode statusCode, string message)
        {
            context.Response.StatusCode = (int)statusCode;
            context.Response.ReasonPhrase = message ?? statusCode.ToString();

            var response = new JObject();

            response.Add("success", false);
            if (!string.IsNullOrEmpty(message))
                response.Add("message", message);

            return context.Response.WriteAsync(response.ToString(Formatting.None));
        }

        public Task Redirect(IOwinContext context, Uri url, bool permenant)
        {
            AddHeader(context, "Location", url.ToString());

            if (permenant) 
                return HttpStatus(context, HttpStatusCode.MovedPermanently, "Moved permenantly");

            return HttpStatus(context, HttpStatusCode.TemporaryRedirect, "Temporary redirect");
        }

        public void AddHeader(IOwinContext context, string name, string value)
        {
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
