﻿using System;
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
    /// This is the default serializer, it sends the response in Json format
    /// </summary>
    public class Json: IRequestDeserializer, IResponseSerializer
    {
        protected JsonSerializerSettings Settings;

        public Json()
        {
            Settings = new JsonSerializerSettings();
            Settings.DefaultValueHandling = DefaultValueHandling.Ignore;
            Settings.Formatting = Formatting.None;
            Settings.MissingMemberHandling = MissingMemberHandling.Ignore;
            Settings.NullValueHandling = NullValueHandling.Ignore;
        }

        public T Body<T>(IOwinContext context)
        {
            using (var reader = new StreamReader(context.Request.Body))
            {
                var content = reader.ReadToEnd();

                T result;
                try
                {
                    result = JsonConvert.DeserializeObject<T>(content, Settings);
                }
                catch(Exception e)
                {
                    throw new BodyDeserializationException(
                        typeof(T), 
                        "A JSON serialization of object type " + typeof(T).DisplayName(),
                        e.Message);
                }

                if (result == null)
                    throw new BodyDeserializationException(
                        typeof(T), 
                        "A JSON serialization of object type " + typeof(T).DisplayName(),
                        "The Newtonsoft deserializer returned null which probably means that " +
                        "you did not post a body. The body must be a JSON serialization of " + 
                        typeof(T).DisplayName());

                return result;
            }
        }

        public Task Success(IOwinContext context)
        {
            context.Response.ContentType = "application/json";
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
                json = JsonConvert.SerializeObject(data, Settings);
            }
            context.Response.ContentType = "application/json";
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

            var response = new JObject 
            {
                { "success", true }, 
                { "redirect", true }, 
                { "location", location }, 
                { "message", message }
            };
            return context.Response.WriteAsync(response.ToString(Formatting.None));
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
