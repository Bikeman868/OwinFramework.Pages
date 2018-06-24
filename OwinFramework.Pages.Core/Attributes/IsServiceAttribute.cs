using System;

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
        public IsServiceAttribute(string name)
            : base(name)
        {
        }
    }
}
