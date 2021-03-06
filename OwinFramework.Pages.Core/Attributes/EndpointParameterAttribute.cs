﻿using System;
using OwinFramework.Pages.Core.Enums;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to a method within a service definition.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter, AllowMultiple = true, Inherited = true)]
    public class EndpointParameterAttribute : Attribute
    {
        /// <summary>
        /// Constructs an attribute that defines a parameter that is expected by a service endpoint.
        /// This version of the constructor is for the case where the attribute is attached to a method
        /// </summary>
        /// <param name="parameterName">The name of the parameter. This will be the name
        /// if the query string parameter, custom header, form field etc</param>
        /// <param name="parserType">A type that provides parsing and validation. This type must have a default public constructor</param>
        /// <param name="parameterType">Bit flags indicating which methods of providing the parameter are allowed</param>
        public EndpointParameterAttribute(
            string parameterName, 
            Type parserType,
            EndpointParameterType parameterType = EndpointParameterType.QueryString)
        {
            ParameterName = parameterName;
            ParserType = parserType;
            ParameterType = parameterType;
        }

        /// <summary>
        /// Constructs an attribute that defines a parameter that is expected by a service endpoint
        /// This version of the constructor is for the case where the attribute is attached to a parameter
        /// </summary>
        /// <param name="parameterType">Bit flags indicating which methods of providing the parameter are allowed</param>
        public EndpointParameterAttribute(
            EndpointParameterType parameterType = EndpointParameterType.QueryString)
        {
            ParameterType = parameterType;
        }

        /// <summary>
        /// This is the name of the parameter that can be passed to the endpoint
        /// </summary>
        public string ParameterName { get; set; }

        /// <summary>
        /// Specifies all of the ways that the parameter is allowed to be passed. You should
        /// not pass the same parameter in multiple ways in one request.
        /// </summary>
        public EndpointParameterType ParameterType { get; set; }

        /// <summary>
        /// Specifies the class to use to parse and validate the parameter value. This can be a value type
        /// in which case the parameter will be parsed as that type. It can also be a nullable
        /// value type which makes it optional. You can also pass a type of parameter parser
        /// </summary>
        public Type ParserType { get; set; }

        /// <summary>
        /// Documentation on the usage of this parameter
        /// </summary>
        public string Description { get; set; }
    }
}
