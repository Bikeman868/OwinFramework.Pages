using System;
using OwinFramework.Pages.Core.Enums;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to a method within a service definition.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class EndpointAttribute : Attribute
    {
        /// <summary>
        /// Constructs an attribute that defines a method to be a service endpoint. This
        /// method will be called in response to requests for its configured url path.
        /// </summary>
        public EndpointAttribute()
        {
            Analytics = AnalyticsLevel.Basic;
        }

        /// <summary>
        /// This can be the absolute path of the endpoint or a relative path. When this is relative
        /// the BasePath of the service is prepended to make an absolute path.
        /// The path can contain dynamic path elements using curly braces. In this case you should
        /// add EndpointParameter attributes to specify how to parse and validate the contents
        /// of this path element.
        /// The path can contain segments that are "*" in which case these path segments will not be
        /// compared when routing requests to this endpoint.
        /// The combined absolute path can end with "/**" in which case all sub-paths match this
        /// endpoint.
        /// If you do not set this property it defaults to the name of the method itself with no
        /// path separator
        /// </summary>
        /// <example>/user/{userid}/**</example>
        /// <example>/customer/{customerid}/invoice/{year}/{month}</example>
        public string UrlPath { get; set; }

        /// <summary>
        /// The http methods that should be routed to this endpoint or an empty list to route
        /// all methods defined by the service
        /// </summary>
        public Method[] Methods { get; set; }

        /// <summary>
        /// How detailed are the analytics for this endpoint
        /// </summary>
        public AnalyticsLevel Analytics { get; set; }

        /// <summary>
        /// Specifies the class to use to deserialize the body of the request. If you do not
        /// provide this value then the deserializer configured for the service will be used
        /// </summary>
        /// <remarks>The type set into this property must implement the IRestRequestDeserializer
        /// interface</remarks>
        public Type RequestDeserializer { get; set; }

        /// <summary>
        /// Specifies the class to use to serialize the response into the body of the reply. 
        /// If you do not provide this value then the serializer configured for the service
        /// will be used.
        /// </summary>
        /// <remarks>The type set into this property must implement the IRestResultSerializer
        /// interface</remarks>
        public Type ResponseSerializer { get; set; }

        /// <summary>
        /// If you provide a value for this attribute then the Identification and Authorization
        /// middleware will reject the request and return 403 if the caller does not have this
        /// permission
        /// </summary>
        public string RequiredPermission { get; set; }

        /// <summary>
        /// When this property is true and the RequiredPermission property is set, the name of the
        /// service and endpoint that is being called is supplied to the Authorization middleware 
        /// as the asset path to check permissions on. This allows you to define one permission
        /// with some users able to call any method and other users only specific subset of services
        /// or endpoints
        /// </summary>
        public bool EndpointSpecificPermission { get; set; }

        /// <summary>
        /// Setting this property to the name of a parameter to the endpoint will cause the value
        /// of that parameter to be passed to the Authorization middleware as the resource name so
        /// that you can grant access to groups of users for specific values in thie parameter only
        /// </summary>
        /// <remarks>Note that this is not implemented yet</remarks>
        public string ParameterSpecificPermission { get; set; } // TODO: Not implemented yet

        /// <summary>
        /// Allows you to specify that this endpoint should be called for a specific 
        /// A/B testing scenario. Traditionally A/B testing was just a UI thing but in this 
        /// framework you can also A/B test service endpoints.
        /// </summary>
        public string ScenarioName { get; set; }
    }
}
