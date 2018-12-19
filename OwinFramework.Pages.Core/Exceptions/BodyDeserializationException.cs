using System;
using OwinFramework.Pages.Core.Extensions;

namespace OwinFramework.Pages.Core.Exceptions
{
    /// <summary>
    /// This exception is thrown when invalid parameter values are
    /// passed to REST service endpoints
    /// </summary>
    public class BodyDeserializationException: Exception
    {
        /// <summary>
        /// The data type of the request body
        /// </summary>
        public Type BodyType { get; private set; }

        /// <summary>
        /// A description of what is valid for this parameter
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// The reason why this parameter is invalid
        /// </summary>
        public string ValidationError { get; private set; }

        /// <summary>
        /// Constructs a new BodyDeserializationException
        /// </summary>
        public BodyDeserializationException(
            Type bodyType,
            string description,
            string errorMessage)
            : base("Invalid body posted with request, expecting '" + bodyType.DisplayName() + "'. " + description)
        {
            BodyType = bodyType;
            Description = description;
            ValidationError = errorMessage;
        }
    }
}
