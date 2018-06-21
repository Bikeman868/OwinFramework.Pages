using System;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// Defines the fluent syntax for building website pages
    /// </summary>
    public interface IPageDefinition
    {
        /// <summary>
        /// Gives a name to the page
        /// </summary>
        IPageDefinition Name(string name);

        /// <summary>
        /// Sets the title for this page
        /// </summary>
        /// <param name="title"></param>
        IPageDefinition Title(string title);

        /// <summary>
        /// Sets the title for this page
        /// </summary>
        /// <param name="titleFunc">A delegate that will calculate the 
        /// title for this page</param>
        IPageDefinition Title(Func<IRenderContext, IDataContext, string> titleFunc);

        /// <summary>
        /// Specifies that this page is part of a package and should
        /// generate and reference assets from that packages namespace
        /// </summary>
        /// <param name="package">The package that this page is
        /// part of</param>
        IPageDefinition PartOf(IPackage package);

        /// <summary>
        /// Specifies that this page is part of a package and should
        /// generate and reference assets from that packages namespace
        /// </summary>
        /// <param name="packageName">The name of the package that this 
        /// page is part of</param>
        IPageDefinition PartOf(string packageName);

        /// <summary>
        /// Specifies that this page is deployed as part of a module
        /// </summary>
        /// <param name="module">The module that this page is deployed in</param>
        IPageDefinition DeployIn(IModule module);

        /// <summary>
        /// Specifies that this page is deployed as part of a module
        /// </summary>
        /// <param name="moduleName">The name of the module that this 
        /// layout is deployed in</param>
        IPageDefinition DeployIn(string moduleName);

        /// <summary>
        /// Sets the css style of the body tag
        /// </summary>
        /// <param name="cssStyle">A valid css style definition, for example "margin: 10;"</param>
        /// <returns></returns>
        IPageDefinition BodyStyle(string cssStyle);

        /// <summary>
        /// Overrides the default asset deployment scheme for this page
        /// </summary>
        IPageDefinition AssetDeployment(AssetDeployment assetDeployment);

        /// <summary>
        /// Specifies the module that this page will be deployed with
        /// </summary>
        IPageDefinition Module(IModule module);

        /// <summary>
        /// Specifies the name of the module that this page will be deployed with
        /// </summary>
        IPageDefinition Module(string moduleName);

        /// <summary>
        /// Specifies the relative path to this page on the website
        /// </summary>
        /// <param name="path">The URL path to this page</param>
        /// <param name="priority">The priority is used to sort request filters.
        /// Higher priority filters execute before lower priority ones</param>
        /// <param name="methods">The http methods to route to this page</param>
        IPageDefinition Route(string path, int priority, params Methods[] methods);

        /// <summary>
        /// Specifies how to filter out requests to this page on the website
        /// </summary>
        /// <param name="filter">Serve this page for requests that match this filter</param>
        /// <param name="priority">Filters are evaluated from highest to lowest priority</param>
        IPageDefinition Route(IRequestFilter filter, int priority = 0);

        /// <summary>
        /// Defaines the layout of this page. If no layout is specified
        /// then the page will have the default layout which has one unanmed
        /// region
        /// </summary>
        IPageDefinition Layout(ILayout layout);

        /// <summary>
        /// Defines the name of the layout of this page. If no layout is specified
        /// then the page will have the default layout which has one unanmed
        /// region
        /// </summary>
        IPageDefinition Layout(string name);

        /// <summary>
        /// Overrides the default contents of one of the regions in the page
        /// layout with a specific component
        /// </summary>
        IPageDefinition RegionComponent(string regionName, IComponent component);

        /// <summary>
        /// Overrides the default contents of one of the regions in the page
        /// layout with a named component
        /// </summary>
        IPageDefinition RegionComponent(string regionName, string componentName);

        /// <summary>
        /// Overrides the default contents of one of the regions in the page
        /// layout with a specific layout
        /// </summary>
        IPageDefinition RegionLayout(string regionName, ILayout layout);

        /// <summary>
        /// Overrides the default contents of one of the regions in the page
        /// layout with a named layout
        /// </summary>
        IPageDefinition RegionLayout(string regionName, string layoutName);

        /// <summary>
        /// Adds metadata to the component that can be queried to establish
        /// its data needs. You can call this more than once to add more than
        /// one type of required data.
        /// </summary>
        /// <typeparam name="T">The type of data that this component binds to.
        /// Provides context for data binding expressions within the component</typeparam>
        IPageDefinition BindTo<T>() where T : class;

        /// <summary>
        /// Adds metadata to the component that can be queried to establish
        /// its data needs. You can call this more than once to add more than
        /// one type of required data.
        /// </summary>
        IPageDefinition BindTo(Type dataType);

        /// <summary>
        /// Specifies the data scope. This will be used to identify the appropriate
        /// data providers
        /// </summary>
        /// <param name="scopeName">The name of the data scope to use when resolving data providers</param>
        IPageDefinition DataScope(string scopeName);

        /// <summary>
        /// Specifies the name of a data provider. This is used as the first step in
        /// resolving data provision. If these providers do not provide all of the required
        /// data then there is a second step of finding data providers using the scope.
        /// </summary>
        /// <param name="providerName">The name of a specific data provider</param>
        IPageDefinition DataProvider(string providerName);

        /// <summary>
        /// Specifies a data provider that is required to establish the data context for
        /// this page
        /// </summary>
        /// <param name="dataProvider">The data provider that is required for this page</param>
        IPageDefinition DataProvider(IDataProvider dataProvider);

        /// <summary>
        /// Specifies a component that renders output to the page that this element
        /// depends on. For example if you have a component that renders a link to the
        /// Boostrap library, any other components that use Bootstrap can ensure it is
        /// included on the page.
        /// </summary>
        /// <param name="componentName">The name of the component that this element depends on</param>
        IPageDefinition NeedsComponent(string componentName);

        /// <summary>
        /// Specifies a component that renders output to the page that this element
        /// depends on. For example if you have a component that renders a link to the
        /// Boostrap library, any other components that use Bootstrap can ensure it is
        /// included on the page.
        /// </summary>
        /// <param name="component">The component that this element depends on</param>
        IPageDefinition NeedsComponent(IComponent component);

        /// <summary>
        /// Builds the page
        /// </summary>
        IPage Build();
    }
}
