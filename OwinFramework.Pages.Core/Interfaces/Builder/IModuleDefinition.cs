using Microsoft.Owin;
using OwinFramework.Pages.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// Defines the fluent syntax for building modules
    /// </summary>
    public interface IModuleDefinition
    {
        /// <summary>
        /// Sets the name of the module so that it can be referenced
        /// by page definitions
        /// </summary>
        IModuleDefinition Name(string name);

        /// <summary>
        /// Overrides the default asset deployment scheme for this module
        /// </summary>
        IModuleDefinition AssetDeployment(AssetDeployment assetDeployment);

        /// <summary>
        /// Builds the module
        /// </summary>
        IModule Build();
    }
}
