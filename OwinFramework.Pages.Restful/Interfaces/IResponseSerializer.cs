using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace OwinFramework.Pages.Restful.Interfaces
{
    /// <summary>
    /// Response serializers are responsible for taking the result
    /// of a REST service call and writing the result to the response
    /// stream.
    /// The IResponseSerializer is responsible for all writing to the
    /// response stream and is fully in controls of all headers and
    /// contet written
    /// </summary>
    public interface IResponseSerializer
    {
        /// <summary>
        /// Serializes a success result to the response
        /// </summary>
        /// <param name="context">The Owin context to write to</param>
        Task Success(IOwinContext context);

        /// <summary>
        /// Serializes a success result with data to the response
        /// </summary>
        /// <param name="context">The Owin context to write to</param>
        /// <param name="data">The data to serialize</param>
        Task Success<T>(IOwinContext context, T data);

        /// <summary>
        /// Serializes a Http status code as the response
        /// </summary>
        /// <param name="context">The Owin context to write to</param>
        /// <param name="statusCode">The status code to send</param>
        /// <param name="message">An optional message associated with the status</param>
        Task HttpStatus(IOwinContext context, HttpStatusCode statusCode, string message = null);

        /// <summary>
        /// Sends a response to the browser instructing it to get this content
        /// from another location
        /// </summary>
        /// <param name="context">The Owin context to write to</param>
        /// <param name="url">The url to redirect the browser to. Note that if you pass
        /// a relative path in response to a POST some browsers repeat the POST
        /// at the new location. To be safe always pass an absolute URL.</param>
        /// <param name="statusCode">It only makes sense to pass a status code 
        /// that expects a location header, i.e. HttpStatusCode.MovedPermanently,
        /// HttpStatusCode.TemporaryRedirect or HttpStatusCode.Found.
        /// Be very careful about passing HttpStatusCode.MovedPermanently here, 
        /// browsers will cache this for a very long time, and there is no way 
        /// for you to clear it</param>
        Task Redirect(IOwinContext context, Uri url, HttpStatusCode statusCode = HttpStatusCode.Found);

        /// <summary>
        /// Adds a custom header to the response
        /// </summary>
        /// <param name="context">The Owin context to write to</param>
        /// <param name="name">The name of the custom header to set</param>
        /// <param name="value">The value to send back in this header</param>
        void AddHeader(IOwinContext context, string name, string value);

        /// <summary>
        /// Stores a cookie on the browser
        /// </summary>
        /// <param name="context">The Owin context to write to</param>
        /// <param name="name">The name of the custom header to set</param>
        /// <param name="value">The value to send back in this header</param>
        /// <param name="expiry">How long the cookie should live for on the browser</param>
        /// <param name="domain">Optional domain specifier</param>
        /// <param name="secure">Pass true to instruct the browser to only send this cookie 
        /// with secure requests (over https)</param>
        void SetCookie(IOwinContext context, string name, string value, TimeSpan expiry, string domain = null, bool secure = false);

        /// <summary>
        /// Deletes a cookie from the browser
        /// </summary>
        /// <param name="context">The Owin context to write to</param>
        /// <param name="name">The name of the custom header to set</param>
        /// <param name="domain">Optional domain specifier</param>
        void DeleteCookie(IOwinContext context, string name, string domain = null);
    }
}
