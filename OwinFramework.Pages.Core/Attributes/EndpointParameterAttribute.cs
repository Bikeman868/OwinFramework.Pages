using System;
using OwinFramework.Pages.Core.Enums;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to a method within a service definition.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class EndpointParameterAttribute : Attribute
    {
        /// <summary>
        /// Constructs an attribute that defines a parameter that is expected by a service endpoint
        /// </summary>
        public EndpointParameterAttribute(
            string parameterName, 
            Type validation,
            EndpointParameterType parameterType = EndpointParameterType.FormField | EndpointParameterType.QueryString)
        {
            ParameterName = parameterName;
            ParameterType = parameterType;
            Validation = validation;
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
        /// Specifies the class to use to validate the parameter value. This can be a value type
        /// in which case the parameter will be parsed as that type. It can also be a nullable
        /// value type which makes it optional. You can also pass a type of validator that
        /// implements the validation interface specific to the service builder you are using
        /// </summary>
        public Type Validation { get; set; }
    }
}
