using System;

namespace OwinFramework.Pages.Core.Enums
{
    /// <summary>
    /// Defines the ways in which a parameter can be passed to a service
    /// endpoint.
    /// </summary>
    [Flags]
    public enum EndpointParameterType
    {
        /// <summary>
        /// The field can be passed in a form field. Form fields are
        /// input elements in html that are included when forms are
        /// POSTed by a Submit button.
        /// </summary>
        FormField = 1,

        /// <summary>
        /// The field can be passed as a query string parameter in the url
        /// </summary>
        QueryString = 2,

        /// <summary>
        /// The field can be passed in a segment of the path in the url
        /// </summary>
        PathSegment = 4,

        /// <summary>
        /// The field can be passed as a custom header. This can be useful
        /// for extracting header values and validating them as input
        /// parameters to a REST service endpoint (for example X-REAL-IP). 
        /// You can also define your own custom headers but you must be 
        /// careful not to violate http protocol standards.
        /// </summary>
        Header = 8
    }
}
