using System;
using OwinFramework.Pages.Core.Enums;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to a class to indicate that it configures a module
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class IsModuleAttribute: Attribute
    {
        /// <summary>
        /// Constructs an attribute that identifies a class as a module. Modukles are deployment
        /// boundaries. All of the css and Javascript for a module would normally reside within 
        /// one file although other deployment scenarios are supported.
        /// </summary>
        /// <param name="moduleName">The name of the module. Must be unique accross the 
        /// whole website</param>
        public IsModuleAttribute(string moduleName)
        {
            Name = moduleName;
        }

        /// <summary>
        /// Constructs an attribute that identifies a class as a module
        /// </summary>
        /// <param name="moduleName">The name of the module. Must be unique accross the 
        /// whole website</param>
        /// <param name="assetDeployment">Defines how this module is deployed</param>
        public IsModuleAttribute(string moduleName, AssetDeployment assetDeployment)
        {
            Name = moduleName;
            AssetDeployment = assetDeployment;
        }

        /// <summary>
        /// Defines an optional name for this module so that is can be referenced 
        /// by name in other elements
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Defines the default deployment method for this module
        /// </summary>
        public AssetDeployment AssetDeployment { get; set; }
    }
}
