using System;
using OwinFramework.Pages.Core.Enums;

namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// Fluent interface for defining layouts. Layouts have:
    /// * An optional name so that they can be referenced
    /// * One or more regions. Regions can be grouped into containers
    /// * Default region content can be overriden with a component or layout
    /// * Asset deployment mechanism can be overriden for children
    /// * The entire layout can be enclosed in an html element 
    /// </summary>
    public interface ILayoutDefinition
    {
        /// <summary>
        /// Sets the name of the layout so that it can be referenced
        /// by name when configuring pages
        /// </summary>
        ILayoutDefinition Name(string name);

        /// <summary>
        /// Specifies that this layout is part of a package and should
        /// generate and reference assets from that packages namespace
        /// </summary>
        /// <param name="package">The package that this layout is
        /// part of</param>
        ILayoutDefinition PartOf(IPackage package);

        /// <summary>
        /// Specifies that this layout is part of a package and should
        /// generate and reference assets from that packages namespace
        /// </summary>
        /// <param name="packageName">The name of the package that this 
        /// layout is part of</param>
        ILayoutDefinition PartOf(string packageName);

        /// <summary>
        /// Specifies that this layout is deployed as part of a module
        /// </summary>
        /// <param name="module">The module that this layout is deployed in</param>
        ILayoutDefinition DeployIn(IModule module);

        /// <summary>
        /// Specifies that this layout is deployed as part of a module
        /// </summary>
        /// <param name="moduleName">The name of the module that this 
        /// layout is deployed in</param>
        ILayoutDefinition DeployIn(string moduleName);

        /// <summary>
        /// Adds a static asset to this component
        /// </summary>
        /// <param name="cssSelector">The selector for this style</param>
        /// <param name="cssStyle">Style to apply when this selector matches elements</param>
        ILayoutDefinition DeployCss(string cssSelector, string cssStyle);

        /// <summary>
        /// Specifies that this layout is deployed as part of a module
        /// </summary>
        /// <param name="returnType">Optional return type of this function. For example "void"</param>
        /// <param name="functionName">The name of this function. For example "getData"</param>
        /// <param name="parameters">TOptional parameters to this function. For example "id, name"</param>
        /// <param name="functionBody">The body of this function. For example "alert('Hello, world');"</param>
        /// <param name="isPublic">Pass true to export this function from the package namespace</param>
        ILayoutDefinition DeployFunction(string returnType, string functionName, string parameters, string functionBody, bool isPublic);

        /// <summary>
        /// Defines how regions are nested. By default regions are rendered one
        /// after the other using whatever html is produced by the region.
        /// Calling this method introduces additional regions as defined by the
        /// layout to contain some of the contained regions.
        /// For example if the layout renders a table then the RegionNesting
        /// could specify the regions to have in each row like this
        /// "(r1,r2)(r3)(r4,r5)" which would create 3 rows
        /// </summary>
        ILayoutDefinition RegionNesting(string regionNesting);

        /// <summary>
        /// Overrides the default asset deployment scheme for this layout
        /// </summary>
        ILayoutDefinition AssetDeployment(AssetDeployment assetDeployment);

        /// <summary>
        /// Defines how to render one of the regions of the layout. If you 
        /// do not specify how to produce the region it will be rendered
        /// using the default region.
        /// </summary>
        ILayoutDefinition Region(string regionName, IRegion region);

        /// <summary>
        /// Defines the name of the region element to use to render a region. If you 
        /// do not specify how to produce the region it will be rendered
        /// using the default region.
        /// </summary>
        ILayoutDefinition Region(string regionName, string name);

        /// <summary>
        /// Overrides the component to place in a region. The region can have
        /// a default component inside it in which case this call will override
        /// that for this specific instance on this layout.
        /// </summary>
        ILayoutDefinition Component(string regionName, IComponent component);

        /// <summary>
        /// Overrides the named component to place in a region
        /// </summary>
        ILayoutDefinition Component(string regionName, string componentName);

        /// <summary>
        /// Overrides the default region content with a specific layout
        /// </summary>
        ILayoutDefinition Layout(string regionName, ILayout layout);

        /// <summary>
        /// Overrides the default region content with a named layout
        /// </summary>
        ILayoutDefinition Layout(string regionName, string layoutName);

        /// <summary>
        /// Specifies the html tag to render around the regions of
        /// this layout. The default is 'div', which means that the whole
        /// layout will be wrapped in a div. You can pass an empty string 
        /// to remove the element that encloses the regions but this means
        /// you can not set the class name and style
        /// </summary>
        ILayoutDefinition Tag(string tagName);

        /// <summary>
        /// The css class names to add to this region
        /// </summary>
        ILayoutDefinition ClassNames(params string[] classNames);

        /// <summary>
        /// The css style to appy to this region
        /// </summary>
        ILayoutDefinition Style(string style);

        /// <summary>
        /// Specifies the html tag to render around regions grouped by
        /// the RegionNesting property. Defaults to 'div'.
        /// </summary>
        ILayoutDefinition NestingTag(string tagName);

        /// <summary>
        /// The css class names to add to any regions created as a result
        /// of round brackets in the RegionNesting property
        /// </summary>
        ILayoutDefinition NestedClassNames(params string[] classNames);

        /// <summary>
        /// The css style to appy to  any regions created as a result
        /// of round brackets in the RegionNesting property
        /// </summary>
        ILayoutDefinition NestedStyle(string style);

        /// <summary>
        /// Adds metadata to the layout that can be queried to establish
        /// its data needs. You can call this more than once to add more than
        /// one type of required data.
        /// </summary>
        /// <typeparam name="T">The type of data that this layout or its children binds to.
        /// Provides context for data binding expressions within the layout and its children</typeparam>
        /// <param name="scope">Optional scope name used to resolve which data provider
        /// will source the data</param>
        ILayoutDefinition BindTo<T>(string scope = null) where T : class;

        /// <summary>
        /// Adds metadata to the layout that can be queried to establish
        /// its data needs. You can call this more than once to add more than
        /// one type of required data.
        /// </summary>
        /// <param name="dataType">The type of data that this layout wil request</param>
        /// <param name="scope">Optional scope name used to resolve which data provider
        /// will source the data</param>
        ILayoutDefinition BindTo(Type dataType, string scope = null);

        /// <summary>
        /// Specifies the name of a data provider. This is used as the first step in
        /// resolving data provision. If these providers do not provide all of the required
        /// data then there is a second step of finding data providers using the scope.
        /// </summary>
        /// <param name="providerName">The name of a specific data provider</param>
        ILayoutDefinition DataProvider(string providerName);

        /// <summary>
        /// Specifies a data provider that is required to establish the data context for
        /// this layout
        /// </summary>
        /// <param name="dataProvider">The data provider that is required for this layout</param>
        ILayoutDefinition DataProvider(IDataProvider dataProvider);

        /// <summary>
        /// Specifies a component that renders output to the page that this element
        /// depends on. For example if you have a component that renders a link to the
        /// Boostrap library, any other components that use Bootstrap can ensure it is
        /// included on the page.
        /// </summary>
        /// <param name="componentName">The name of the component that this element depends on</param>
        ILayoutDefinition NeedsComponent(string componentName);

        /// <summary>
        /// Specifies a component that renders output to the page that this element
        /// depends on. For example if you have a component that renders a link to the
        /// Boostrap library, any other components that use Bootstrap can ensure it is
        /// included on the page.
        /// </summary>
        /// <param name="component">The component that this element depends on</param>
        ILayoutDefinition NeedsComponent(IComponent component);

        /// <summary>
        /// Builds the layout
        /// </summary>
        ILayout Build();
    }
}
