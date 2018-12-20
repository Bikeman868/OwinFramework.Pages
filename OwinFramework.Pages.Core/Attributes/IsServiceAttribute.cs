using System;
using OwinFramework.Pages.Core.Enums;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to a service that you want to have discovered and 
    /// registered automitically at startup. If your service implements IService
    /// it works out better if you build and register it using a Package instead
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class IsServiceAttribute : IsElementAttributeBase
    {
        /// <summary>
        /// Constructs an attribute that defines a class to be a service
        /// </summary>
        /// <param name="name">The name of the service. Must be unique within a package</param>
        /// <param name="basePath">The part of the URL path that is common to all endpoints in
        /// this service. You can also specify absolute paths for some or all endpoints</param>
        /// <param name="methods">A list of the Http methods that should be routed to
        /// this service. You can pass an empty list and specify this for each endpoint instead</param>
        public IsServiceAttribute(string name, string basePath = null, Method[] methods = null)
            : base(name)
        {
            BasePath = basePath ?? "/";
            MethodsToRoute = methods ?? new[] { Method.Post };
        }

        /// <summary>
        /// For any endpoints that specify a relative path, this path will be prepended to
        /// make the url path absolute.
        /// </summary>
        public string BasePath { get; set; }

        /// <summary>
        /// The http methods that should be routed to this service or an empty list to route
        /// all methods
        /// </summary>
        public Method[] MethodsToRoute { get; set; }

        /// <summary>
        /// The router sorts runables by priority and finds the first one that matches the
        /// request.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Specifies the class to use to deserialize the body of the request. If you do not
        /// provide this value then the default deserializer for the service build engine will
        /// be used. This property defaines the default for all endpoints in the service,
        /// but you can be override this for any endpoint that needs different deserialization
        /// </summary>
        /// <remarks>The type set into this property must implement the IRestRequestDeserializer
        /// interface</remarks>
        public Type RequestDeserializer { get; set; }

        /// <summary>
        /// Specifies the class to use to serialize the response into the body of the reply. 
        /// If you do not provide this value then the default serializer for the service 
        /// build engine will be used. This property defaines the default for all endpoints 
        /// in the service, but you can be override this for any endpoint that needs different 
        /// serialization
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
    }
}
