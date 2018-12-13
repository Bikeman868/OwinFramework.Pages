using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwinFramework.Pages.Core.Exceptions
{
    /// <summary>
    /// This exception is thrown when invalid parameter values are
    /// passed to REST service endpoints
    /// </summary>
    public class EndpointParameterException: Exception
    {
        /// <summary>
        /// The name of the parameter to the service endpoint
        /// </summary>
        public string ParameterName { get; private set; }

        /// <summary>
        /// The data type of the parameter
        /// </summary>
        public Type ParameterType { get; private set; }

        /// <summary>
        /// A description of what is valid for this parameter
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// The reason why this parameter is invalid
        /// </summary>
        public string ValidationError { get; private set; }

        /// <summary>
        /// Constructs a new EndpointParameterException
        /// </summary>
        public EndpointParameterException(
            string parameterName,
            Type parameterType,
            string description,
            string errorMessage)
            : base("Invalid parameter value for '" + parameterName + "'. " + description)
        {
            ParameterName = parameterName;
            ParameterType = parameterType;
            Description = description;
            ValidationError = errorMessage;
        }
    }
}
