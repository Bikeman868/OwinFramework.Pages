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
    /// Defines the fluent syntax for building regions
    /// </summary>
    public interface IRegionDefinition
    {
        /// <summary>
        /// Specifies the name of the region so that it can 
        /// be referenced by name when building layouts
        /// </summary>
        IRegionDefinition Name(string name);

        /// <summary>
        /// Overrides the default asset deployment scheme for this region
        /// </summary>
        IRegionDefinition AssetDeployment(AssetDeployment assetDeployment);

        /// <summary>
        /// Specifies the default layout for this region. This can
        /// be overriden for each instance of the region on a layout
        /// </summary>
        IRegionDefinition Layout(ILayout layout);

        /// <summary>
        /// Specifies the default layout name for this region. This can
        /// be overriden for each instance of the region on a layout
        /// </summary>
        IRegionDefinition Layout(string name);

        /// <summary>
        /// Specifies the default component for this region. This can
        /// be overriden for each instance of the region on a layout
        /// </summary>
        IRegionDefinition Component(IComponent component);

        /// <summary>
        /// Specifies the default component ame for this region. This can
        /// be overriden for each instance of the region on a layout
        /// </summary>
        IRegionDefinition Component(string componentName);

        /// <summary>
        /// Builds the region
        /// </summary>
        IRegion Build();
    }
}
