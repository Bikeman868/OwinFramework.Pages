﻿using OwinFramework.Pages.Core.Enums;

namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// Defines the fluent syntax for building regions. The region:
    /// * Can have a name so that is can be referenced
    /// * Can override the asset deployment scheme foe its children
    /// * Can contain a single component or layout
    /// * Can enclose the contents of the region in an html element
    /// * Can have its behaviour controlled by an external style sheet
    /// * Can have a custom style with randomly generated name
    /// * Can produce assets in the namespace of the package
    /// * Can define the data binding context handler to invoke to provide data to the component inside
    /// * Can bind to a list of data objects in context and repeat the region for each object
    /// </summary>
    public interface IRegionDefinition
    {
        /// <summary>
        /// Specifies the name of the region so that it can 
        /// be referenced by name when building layouts
        /// </summary>
        IRegionDefinition Name(string name);

        /// <summary>
        /// Specifies that this region is part of a package and should
        /// generate and reference assets from that packages namespace
        /// </summary>
        /// <param name="package">The package that this layout is
        /// part of</param>
        IRegionDefinition PartOf(IPackage package);

        /// <summary>
        /// Specifies that this region is part of a package and should
        /// generate and reference assets from that packages namespace
        /// </summary>
        /// <param name="packageName">The name of the package that this 
        /// region is part of</param>
        IRegionDefinition PartOf(string packageName);

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
        /// Specifies the html tag to render around the contents of
        /// this region. The default is 'div' if this method is not called
        /// </summary>
        IRegionDefinition Tag(string tagName);

        /// <summary>
        /// The css class names to add to this region
        /// </summary>
        IRegionDefinition ClassNames(params string[] classNames);

        /// <summary>
        /// The css style to appy to this region
        /// </summary>
        IRegionDefinition Style(string style);

        /// <summary>
        /// Causes the region to be rendered multiple times, once
        /// for each object in the data context
        /// </summary>
        /// <typeparam name="T">Looks for a list or enumeration of objects 
        /// of this type in the rendering context and repeats the region
        /// once for each item</typeparam>
        IRegionDefinition ForEach<T>();

        /// <summary>
        /// Builds the region
        /// </summary>
        IRegion Build();
    }
}
