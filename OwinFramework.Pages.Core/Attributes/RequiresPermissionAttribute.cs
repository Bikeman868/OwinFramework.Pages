using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to an element to make it part of a package (namespace)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class RequiresPermissionAttribute: Attribute
    {
        /// <summary>
        /// Constructs and initializes an attribute that specifies the package that 
        /// this element belongs to
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
