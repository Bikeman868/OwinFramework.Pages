using System;

namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// Use the module builder to construct modules using a fluent syntax
    /// </summary>
    public interface IModuleBuilder
    {
        /// <summary>
        /// Starts building a new module or configuring an existing module
        /// </summary>
        /// <param name="moduleInstance">Pass an instance that derives from Module
        /// to configure it directly, or pass any other instance type or null to 
        /// construct an instance of the Module class</param>
        /// <param name="declaringType">Type type to extract custom attributes from
        /// that can also define the behaviour of the module</param>
        IModuleDefinition BuildUpModule(object moduleInstance = null, Type declaringType = null);
    }
}
