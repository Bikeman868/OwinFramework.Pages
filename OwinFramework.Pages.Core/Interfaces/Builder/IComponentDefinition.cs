using System;
using OwinFramework.Pages.Core.Enums;

namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// Defines the fluent syntax for building components. A component:
    /// * Has an optional name so that is can be referenced
    /// * Can override the default asset deployment mechanism
    /// * Can bind to one or more data objects
    /// * Outputs content into various parts of the page (head, body etc)
    /// * Generates unique asset names within the namespace of its package
    /// * Can have dependencies on other components
    /// </summary>
    public interface IComponentDefinition
    {
        /// <summary>
        /// Sets the name of the component so that it can be referenced
        /// by other elements
        /// </summary>
        IComponentDefinition Name(string name);

        /// <summary>
        /// Specifies that this component is part of a package and should
        /// generate and reference assets from that packages namespace
        /// </summary>
        /// <param name="package">The package that this component is
        /// part of</param>
        /// <returns></returns>
        IComponentDefinition PartOf(IPackage package);

        /// <summary>
        /// Specifies that this component is part of a package and should
        /// generate and reference assets from that packages namespace
        /// </summary>
        /// <param name="packageName">The name of the package that this 
        /// component is part of</param>
        /// <returns></returns>
        IComponentDefinition PartOf(string packageName);

        /// <summary>
        /// Specifies that this component is deployed as part of a module
        /// </summary>
        /// <param name="module">The module that this component is deployed in</param>
        IComponentDefinition DeployIn(IModule module);

        /// <summary>
        /// Specifies that this layout is deployed as part of a module
        /// </summary>
        /// <param name="moduleName">The name of the module that this 
        /// layout is deployed in</param>
        IComponentDefinition DeployIn(string moduleName);

        /// <summary>
        /// Adds a static asset to this component
        /// </summary>
        /// <param name="cssSelector">The selector for this style. This selector can include
        /// {ns}_ markers that will be replaced by the namespace of the pachage or removed
        /// when there is no package in context</param>
        /// <param name="cssStyle">Style to apply when this selector matches elements</param>
        IComponentDefinition DeployCss(string cssSelector, string cssStyle);

        /// <summary>
        /// Adds a static asset to this component
        /// </summary>
        /// <param name="cssStyleSheet">A stylesheet defining multiple styles. This style sheet
        /// will be parsed into individual style definitions that will then be processed
        /// individually by the other overload.</param>
        IComponentDefinition DeployCss(string cssStyleSheet);

        /// <summary>
        /// Specifies that this layout is deployed as part of a module
        /// </summary>
        /// <param name="returnType">Optional return type of this function. For example "void"</param>
        /// <param name="functionName">The name of this function. For example "getData"</param>
        /// <param name="parameters">TOptional parameters to this function. For example "id, name"</param>
        /// <param name="functionBody">The body of this function. For example "alert('Hello, world');"</param>
        /// <param name="isPublic">Pass true to export this function from the package namespace</param>
        IComponentDefinition DeployFunction(string returnType, string functionName, string parameters, string functionBody, bool isPublic);

        /// <summary>
        /// Overrides the default asset deployment scheme for this component
        /// </summary>
        IComponentDefinition AssetDeployment(AssetDeployment assetDeployment);

        /// <summary>
        /// Adds metadata to the component that can be queried to establish
        /// its data needs. You can call this more than once to add more than
        /// one type of required data.
        /// </summary>
        /// <typeparam name="T">The type of data that this component binds to.
        /// Provides context for data binding expressions within the component</typeparam>
        /// <param name="scope">Optional scope name used to resolve which data provider
        /// will source the data</param>
        IComponentDefinition BindTo<T>(string scope = null) where T: class;

        /// <summary>
        /// Adds metadata to the component that can be queried to establish
        /// its data needs. You can call this more than once to add more than
        /// one type of required data.
        /// </summary>
        /// <param name="dataType">The type of data that this element wil request</param>
        /// <param name="scope">Optional scope name used to resolve which data provider
        /// will source the data</param>
        IComponentDefinition BindTo(Type dataType, string scope = null);

        /// <summary>
        /// Specifies the name of a data provider. This is used as the first step in
        /// resolving data provision. If these providers do not provide all of the required
        /// data then there is a second step of finding data providers using the scope.
        /// </summary>
        /// <param name="providerName">The name of a specific data provider</param>
        IComponentDefinition DataProvider(string providerName);

        /// <summary>
        /// Specifies a data provider that is required to establish the data context for
        /// this component
        /// </summary>
        /// <param name="dataProvider">The data provider that is required for this component</param>
        IComponentDefinition DataProvider(IDataProvider dataProvider);

        /// <summary>
        /// Specifies a component that renders output to the page that this element
        /// depends on. For example if you have a component that renders a link to the
        /// Boostrap library, any other components that use Bootstrap can ensure it is
        /// included on the page.
        /// </summary>
        /// <param name="componentName">The name of the component that this element depends on</param>
        IComponentDefinition NeedsComponent(string componentName);

        /// <summary>
        /// Specifies a component that renders output to the page that this element
        /// depends on. For example if you have a component that renders a link to the
        /// Boostrap library, any other components that use Bootstrap can ensure it is
        /// included on the page.
        /// </summary>
        /// <param name="component">The component that this element depends on</param>
        IComponentDefinition NeedsComponent(IComponent component);

        /// <summary>
        /// Tells the component to render some html
        /// </summary>
        /// <param name="assetName">The name of the asset must uniquely identify a 
        /// text asset. The asset manager can be used to provdide localized verions</param>
        /// <param name="html">The html to render in the default locale</param>
        IComponentDefinition Render(string assetName, string html);

        /// <summary>
        /// Tells the component to render some html into the page head
        /// </summary>
        /// <param name="assetName">The name of the asset must uniquely identify a 
        /// text asset. The asset manager can be used to provdide localized verions</param>
        /// <param name="html">The html to render in the default locale</param>
        IComponentDefinition RenderHead(string assetName, string html);

        /// <summary>
        /// Tells the component to render some html into the page initialization
        /// </summary>
        /// <param name="assetName">The name of the asset must uniquely identify a 
        /// text asset. The asset manager can be used to provdide localized verions</param>
        /// <param name="html">The html to render in the default locale</param>
        IComponentDefinition RenderInitialization(string assetName, string html);

        /// <summary>
        /// Builds the component
        /// </summary>
        IComponent Build();
    }
}
