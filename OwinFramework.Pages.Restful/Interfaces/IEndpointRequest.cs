using System;
using System.Net;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Enums;

namespace OwinFramework.Pages.Restful.Interfaces
{
    /// <summary>
    /// Defines a request that is being routed to a Restful endpoint
    /// </summary>
    public interface IEndpointRequest
    {
        /// <summary>
        /// Retrieves the Owin context associated with this request
        /// This is generally only needed for more advanced use cases
        /// </summary>
        IOwinContext OwinContext { get; }

        /// <summary>
        /// Retrieves the value of a parameter passed to the service.
        /// You must declare these parameters by attaching EndpointParamater
        /// attributes the service endpoint method before they will be available
        /// </summary>
        /// <typeparam name="T">The type of parameter. Must match the type
        /// parsed by the parameter validator specified in the attribute</typeparam>
        /// <param name="name">Case insensitive name of the parameter</param>
        /// <returns>The value of the parameter</returns>
        T Parameter<T>(string name);

        /// <summary>
        /// Indicates a sucessfull completion of the request. Calling this method
        /// is redundant since this is the default behavior anyway, but can make
        /// your code more readable
        /// </summary>
        void Success();

        /// <summary>
        /// Indicates a sucessfull completion of the request and provides data
        /// to serialize into the body of the response. If the method has a serializer
        /// defined then it will be used to serialize the response, otherwise the
        /// serializer associated with the service will be used, otherwise the
        /// default serialization is JSON.
        /// </summary>
        /// <typeparam name="T">Tye type of data to serialize</typeparam>
        /// <param name="valueToSerialize">The value to serialize</param>
        void Success<T>(T valueToSerialize);

        /// <summary>
        /// Indicates that the endpoint completed with a specific http status
        /// to report.
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="message">The message to include in the http response header</param>
        void HttpStatus(HttpStatusCode statusCode, string message);

        /// <summary>
        /// Returns a 404 (not found) response to the caller
        /// </summary>
        void NotFound();

        /// <summary>
        /// Returns a 204 (no content) response to the caller
        /// </summary>
        void NoContent();

        /// <summary>
        /// Returns a 400 (bad request) response to the caller
        /// </summary>
        void BadRequest();

        /// <summary>
        /// Returns a response to the caller indicating that the user agent
        /// should retry the request at a different URL
        /// </summary>
        /// <param name="url">The Url to redirect to</param>
        /// <param name="permenant">Pass true for permenant redirection</param>
        void Redirect(Uri url, bool permenant = false);

        /// <summary>
        /// Renders 
        /// </summary>
        /// <param name="url">The URL of the page to render as the response</param>
        /// <param name="httpMethod">The HTTP method to use in routing the request</param>
        void Rewrite(Uri url, Methods httpMethod = Methods.Get);
    }
}
