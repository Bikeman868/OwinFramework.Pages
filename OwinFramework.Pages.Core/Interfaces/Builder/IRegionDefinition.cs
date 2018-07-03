using OwinFramework.Pages.Core.Enums;
using System;

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
        /// Specifies that this region is deployed as part of a module
        /// </summary>
        /// <param name="module">The module that this region is deployed in</param>
        IRegionDefinition DeployIn(IModule module);

        /// <summary>
        /// Specifies that this region is deployed as part of a module
        /// </summary>
        /// <param name="moduleName">The name of the module that this 
        /// region is deployed in</param>
        IRegionDefinition DeployIn(string moduleName);

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
        IRegionDefinition Layout(string layoutName);

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
        /// Adds metadata to the region that can be queried to establish
        /// its data needs. You can call this more than once to add more than
        /// one type of required data.
        /// </summary>
        /// <typeparam name="T">The type of data that this page or its contents binds to.
        /// Provides context for data binding expressions within the page and its contents</typeparam>
        /// <param name="scope">Optional scope name used to resolve which data provider
        /// will source the data</param>
        IRegionDefinition BindTo<T>(string scope = null) where T : class;

        /// <summary>
        /// Adds metadata to the page that can be queried to establish
        /// its data needs. You can call this more than once to add more than
        /// one type of required data.
        /// </summary>
        /// <param name="dataType">The type of data that this page wil request</param>
        /// <param name="scope">Optional scope name used to resolve which data provider
        /// will source the data</param>
        IRegionDefinition BindTo(Type dataType, string scope = null);

        /// <summary>
        /// Instructs the region to use a specific scope when locating a data
        /// provider for the specified type of data.
        /// </summary>
        /// <param name="type">The type of data to scope</param>
        /// <param name="scopeName">The name of the data scope to use when resolving data providers</param>
        IRegionDefinition DataScope(Type type, string scopeName);

        /// <summary>
        /// Specifies the name of a data provider. This is used as the first step in
        /// resolving data provision. If these providers do not provide all of the required
        /// data then there is a second step of finding data providers using the scope.
        /// </summary>
        /// <param name="providerName">The name of a specific data provider</param>
        IRegionDefinition DataProvider(string providerName);

        /// <summary>
        /// Specifies a data provider that is required to establish the data context for
        /// this region
        /// </summary>
        /// <param name="dataProvider">The data provider that is required for this region</param>
        IRegionDefinition DataProvider(IDataProvider dataProvider);

        /// <summary>
        /// Specifies a component that renders output to the page that this element
        /// depends on. For example if you have a component that renders a link to the
        /// Boostrap library, any other components that use Bootstrap can ensure it is
        /// included on the page.
        /// </summary>
        /// <param name="componentName">The name of the component that this element depends on</param>
        IRegionDefinition NeedsComponent(string componentName);

        /// <summary>
        /// Specifies a component that renders output to the page that this element
        /// depends on. For example if you have a component that renders a link to the
        /// Boostrap library, any other components that use Bootstrap can ensure it is
        /// included on the page.
        /// </summary>
        /// <param name="component">The component that this element depends on</param>
        IRegionDefinition NeedsComponent(IComponent component);

        /// <summary>
        /// Causes the region to be rendered multiple times, once
        /// for each object in the data context
        /// </summary>
        /// <typeparam name="T">Looks for a list or enumeration of objects 
        /// of this type in the rendering context and repeats the region
        /// once for each item</typeparam>
        IRegionDefinition ForEach<T>(string scopeName = "", string tag = "", string style = "", params string[] classes);

        /// <summary>
        /// Causes the region to be rendered multiple times, once
        /// for each object in the data context of the specified type
        /// </summary>
        IRegionDefinition ForEach(Type dataType, string scopeName = "", string tag = "", string style = "", params string[] classes);

        /// <summary>
        /// Builds the region
        /// </summary>
        IRegion Build();
    }
}
