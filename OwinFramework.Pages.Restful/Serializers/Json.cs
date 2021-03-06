﻿using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Restful.Interfaces;

namespace OwinFramework.Pages.Restful.Serializers
{
    /// <summary>
    /// This is the default serializer, it sends the response in Json format
    /// </summary>
    public class Json : SerializerBase, IRequestDeserializer, IResponseSerializer
    {
        protected JsonSerializerSettings Settings;

        public Json()
        {
            Settings = new JsonSerializerSettings();
            Settings.DefaultValueHandling = DefaultValueHandling.Include;
            Settings.Formatting = Formatting.None;
            Settings.MissingMemberHandling = MissingMemberHandling.Ignore;
            Settings.NullValueHandling = NullValueHandling.Ignore;
            Settings.Converters.Add(new StringEnumConverter
            {
                CamelCaseText = false, 
                AllowIntegerValues = true
            });
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
                json = value.ToString(CultureInfo.InvariantCulture);
            }
            else if (type.IsArray || typeof(IList).IsAssignableFrom(type))
            {
                var value = JArray.FromObject(data);
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

    }
}
