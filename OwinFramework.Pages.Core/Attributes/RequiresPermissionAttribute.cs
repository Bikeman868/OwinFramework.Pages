using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to an element to make it part of a package (namespace)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class RequiresAttribute: Attribute
    {
        /// <summary>
        /// Constructs and initializes an attribute that specifies the package that 
        /// this element belongs to
        /// </summary>
        /// <param name="permissionName">The name of the required permission</param>
        public RequiresAttribute(string permissionName)
        {
            PermissionName = permissionName;
        }

        /// <summary>
        /// The name of the permission
        /// </summary>
        public string PermissionName { get; set; }
    }
}
