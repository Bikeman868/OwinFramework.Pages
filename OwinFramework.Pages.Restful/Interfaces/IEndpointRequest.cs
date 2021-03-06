﻿using System;
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
        /// If the request was not anonymous and there is identiication middleware
        /// in the owin pipeline then this returns the identity of the caller
        /// otherwise returns an empty string
        /// </summary>
        string Identity { get; }

        /********************************************************************
         * 
         * This section provides methods for reading the request.
         * 
         *******************************************************************/

        /// <summary>
        /// Retrieves data from a data provider. For efficiency you should
        /// attach attributes to your endpoint that identify the data needs
        /// </summary>
        /// <typeparam name="T">The type of data to get</typeparam>
        /// <param name="scopeName">Optional scope name will be used to
        /// identify the data provider</param>
        T Data<T>(string scopeName = null);

        /// <summary>
        /// Retrieves the value of a parameter passed to the service.
        /// You must declare these parameters by attaching EndpointParamater
        /// attributes the service endpoint method before they will be available
        /// </summary>
        /// <typeparam name="T">The type of parameter. Must match the type
        /// parsed by the parameter parser specified in the attribute</typeparam>
        /// <param name="name">Case insensitive name of the parameter</param>
        /// <returns>The value of the parameter</returns>
        T Parameter<T>(string name);

        /// <summary>
        /// Retrieves the value of a parameter passed to the service.
        /// You must declare these parameters by attaching EndpointParamater
        /// attributes the service endpoint method before they will be available
        /// </summary>
        /// <param name="name">Case insensitive name of the parameter</param>
        /// <returns>The value of the parameter</returns>
        object GetParameter(string name);

        /// <summary>
        /// Returns a segment from the URL path only parsing the URL on the
        /// first request.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        string PathSegment(int index);

        /// <summary>
        /// Deserializes the body of the request as the specified type. You
        /// can specify the deserializer to use for your service or each endpoint.
        /// This framework contains some useful deserializers or you can
        /// supply an application specific one.
        /// </summary>
        T Body<T>();

        /// <summary>
        /// Parses the body of the request as a POSTed form and returns the
        /// form fields as a dictionary. If you get this property multiple
        /// times the dictionary will only be constructed once.
        /// </summary>
        IFormCollection Form { get; }

        /********************************************************************
         * 
         * This section provides methods for sending a reply
         * 
         *******************************************************************/

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
        /// <param name="valueToSerialize">The value to serialize into the 
        /// body of the response</param>
        void Success<T>(T valueToSerialize);

        /// <summary>
        /// Indicates that the endpoint completed with a specific http status result.
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="message">The message to include in the http response header</param>
        void HttpStatus(HttpStatusCode statusCode, string message = null);

        /// <summary>
        /// Returns a 404 (not found) response to the caller
        /// </summary>
        void NotFound(string message = null);

        /// <summary>
        /// Returns a 204 (no content) response to the caller
        /// </summary>
        void NoContent(string message = null);

        /// <summary>
        /// Returns a 400 (bad request) response to the caller
        /// </summary>
        void BadRequest(string message = null);

        /// <summary>
        /// Returns a response to the caller indicating that the user agent
        /// should retry the request at a different URL
        /// </summary>
        /// <param name="url">The url to redirect the browser to. Can be a relative 
        /// URL to redirect to another page on the same website or an absolute URL
        /// to redirect to a different website.</param>
        /// <param name="statusCode">It only makes sense to pass a status code 
        /// that expects a location header, i.e. HttpStatusCode.MovedPermanently,
        /// HttpStatusCode.TemporaryRedirect or HttpStatusCode.Found.
        /// Note that when redirecting after a POST TemporaryRedirect will retry the
        /// POST at the new URL whereas HttpStatusCode.Found will perform a GET request.
        /// Be very careful about passing HttpStatusCode.MovedPermanently here, 
        /// browsers will cache this for a very long time, and there is no way 
        /// for you to clear it</param>
        void Redirect(Uri url, HttpStatusCode statusCode = HttpStatusCode.Found);

        /// <summary>
        /// Passes this request back to the request router. The request router will
        /// resolve the URL to a page or service that will provide the response
        /// </summary>
        /// <param name="path">The path to the page to render as the response. If you do
        /// not pass this parameter then the request will be rewritten into a GET
        /// requet at the same url. This is a useful behavior when you have a submit
        /// button on a form and want to render the same page again in response.</param>
        /// <param name="httpMethod">The HTTP method to use in routing the request</param>
        void Rewrite(string path = null, Method httpMethod = Method.Get);
    }
}
