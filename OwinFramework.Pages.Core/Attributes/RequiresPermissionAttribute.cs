using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to an element to specify that the caller must
    /// identify themselves and their account must be assigned a specific permission
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class RequiresPermissionAttribute: Attribute
    {
        /// <summary>
        /// Constructs and initializes an attribute that specifies the permission that
        /// a user must have to call this endpoint
        /// </summary>
        /// <param name="permissionName">The name of the required permission</param>
        /// <param name="resourcePath">Optional path to the protected resource</param>
        public RequiresPermissionAttribute(string permissionName, string resourcePath = null)
        {
            PermissionName = permissionName;
            ResourcePath = resourcePath;
        }

        /// <summary>
        /// The name of the permission
        /// </summary>
        public string PermissionName { get; set; }

        /// <summary>
        /// Optional path to the protected resource
        /// </summary>
        public string ResourcePath { get; set; }
    }
}
