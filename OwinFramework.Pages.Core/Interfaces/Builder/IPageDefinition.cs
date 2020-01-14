using System;
using OwinFramework.InterfacesV1.Middleware;
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
        IPageDefinition Title(Func<IRenderContext, string> titleFunc);

        /// <summary>
        /// Sets the canonical Url for this page
        /// </summary>
        /// <param name="canonicalUrl"></param>
        IPageDefinition CanonicalUrl(string canonicalUrl);

        /// <summary>
        /// Sets the title for this page
        /// </summary>
        /// <param name="canonicalUrlFunc">A delegate that will calculate the 
        /// canonical Url for this page</param>
        IPageDefinition CanonicalUrl(Func<IRenderContext, string> canonicalUrlFunc);
        
        /// <summary>
        /// Sets the css style of the body tag
        /// </summary>
        /// <param name="cssStyle">A valid css style definition, for example "margin: 10;"</param>
        /// <returns></returns>
        IPageDefinition BodyStyle(string cssStyle);

        /// <summary>
        /// Specifies the relative path to this page on the website
        /// </summary>
        /// <param name="path">The URL path to this page</param>
        /// <param name="priority">The priority is used to sort request filters.
        /// Higher priority filters execute before lower priority ones</param>
        /// <param name="methods">The http methods to route to this page</param>
        IPageDefinition Route(string path, int priority, params Method[] methods);

        /// <summary>
        /// Specifies how to filter out requests to this page on the website
        /// </summary>
        /// <param name="filter">Serve this page for requests that match this filter</param>
        /// <param name="priority">Filters are evaluated from highest to lowest priority</param>
        IPageDefinition Route(IRequestFilter filter, int priority = 0);

        /// <summary>
        /// Tells the authentication middleware that the caller must have a permission
        /// assigned to them to be allowed to access this page. When you set this
        /// property you must have some Authorization middleware installed and configured
        /// to run before the Pages middleware in the Owin pipeline.
        /// </summary>
        /// <param name="permissionName">The name of the permission that must be assigned
        /// to the user in the Authentication middleware</param>
        /// <param name="assetName">Optional additional information used to test the users
        /// permission. Asset restictions can be set up in the Authorization middleware.</param>
        IPageDefinition RequiresPermission(string permissionName, string assetName = null);

        /// <summary>
        /// Tells the authentication middleware that the caller must be identified to
        /// access this page
        /// </summary>
        IPageDefinition RequiresIdentification();

        /// <summary>
        /// Defines information that can be used by the Output Cache middleware to cache
        /// this page. The output cache can cache the generated Html for a period of time
        /// and can also instruct the browser, CDN and proxy services to cache the page.
        /// By default the page will not be cached anywhere on the assumption that pages
        /// contain dynamic data that changes with every request.
        /// </summary>
        /// <param name="cacheCategory">This category name if passed to the output
        /// cache middleware. This type of middleware typically has rules defined based
        /// on category and priority that define what to cache, how long for and what
        /// to retain in cache when there is memory pressure.</param>
        /// <param name="cachePriority">Defines how expensive it is to produce this
        /// page and therefore how much benefit there is to caching it.</param>
        IPageDefinition Cache(string cacheCategory, CachePriority cachePriority);

        #region Packaging and deployment

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
        /// Overrides the default asset deployment scheme for this page
        /// </summary>
        IPageDefinition AssetDeployment(AssetDeployment assetDeployment);

        #endregion

        #region Page layout

        /// <summary>
        /// Defines the layout of this page. If no layout is specified
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
        /// Overrides the default contents of one of the zones in the page
        /// layout with a specific component
        /// </summary>
        /// <param name="zoneName">The name of the region within the layout</param>
        /// <param name="component">The component element to put in this area of the
        /// layout for this page</param>
        IPageDefinition ZoneComponent(string zoneName, IComponent component);

        /// <summary>
        /// Overrides the default contents of one of the zones in the page
        /// layout with a named component
        /// </summary>
        IPageDefinition ZoneComponent(string zoneName, string componentName);

        /// <summary>
        /// Overrides the default contents of one of the zones in the page
        /// layout with a specific layout
        /// </summary>
        /// <param name="zoneName">The name of the region within the layout</param>
        /// <param name="layout">The layout element to put in this area of the
        /// layout for this page</param>
        IPageDefinition ZoneLayout(string zoneName, ILayout layout);

        /// <summary>
        /// Overrides the default contents of one of the regions in the page
        /// layout with a named layout
        /// </summary>
        /// <param name="zoneName">The name of the region within the layout</param>
        /// <param name="layoutName">The name of the layout to put in this area of the
        /// layout for this page</param>
        IPageDefinition ZoneLayout(string zoneName, string layoutName);

        /// <summary>
        /// Overrides the default contents of one of the regions in the page
        /// layout with a specific region
        /// </summary>
        /// <param name="zoneName">The name of the region within the layout</param>
        /// <param name="region">The region element to put in this area of the
        /// layout for this page</param>
        IPageDefinition ZoneRegion(string zoneName, IRegion region);

        /// <summary>
        /// Overrides the default contents of one of the zones in the page
        /// layout with a named region
        /// </summary>
        /// <param name="zoneName">The name of the region within the layout</param>
        /// <param name="regionName">The name of the region element to put into this
        /// zone of the layout for this page</param>
        IPageDefinition ZoneRegion(string zoneName, string regionName);

        /// <summary>
        /// Populates a region of the layout with static Html avoiding the need
        /// to define a region and a component for very simple use cases. A region 
        /// and a component will be generated internally with default properties.
        /// </summary>
        /// <param name="zoneName">The name of the region within the layout</param>
        /// <param name="textAssetName">The name of the text asset to localize</param>
        /// <param name="defaultHtml">The default Html for all unsupported locales.
        /// Note that if you did not setup localization then this will be the html
        /// for all locales.</param>
        IPageDefinition ZoneHtml(string zoneName, string textAssetName, string defaultHtml);

        /// <summary>
        /// Populates a zone of the layout with a template avoiding the need
        /// to define a region and a component. A region and a component will be
        /// generated internally with default properties.
        /// </summary>
        /// <param name="zoneName">The name of the region within the layout</param>
        /// <param name="templatePath">A / separated path to the template to load
        /// into this region of the layout</param>
        IPageDefinition ZoneTemplate(string zoneName, string templatePath);
        
        #endregion

        #region Data binding

        /// <summary>
        /// Adds metadata to the page that can be queried to establish
        /// its data needs. You can call this more than once to add more than
        /// one type of required data.
        /// </summary>
        /// <typeparam name="T">The type of data that this page or its contents binds to.
        /// Provides context for data binding expressions within the page and its contents</typeparam>
        /// <param name="scope">Optional scope name used to resolve which data provider
        /// will source the data</param>
        IPageDefinition BindTo<T>(string scope = null) where T : class;

        /// <summary>
        /// Adds metadata to the page that can be queried to establish
        /// its data needs. You can call this more than once to add more than
        /// one type of required data.
        /// </summary>
        /// <param name="dataType">The type of data that this page wil request</param>
        /// <param name="scope">Optional scope name used to resolve which data provider
        /// will source the data</param>
        IPageDefinition BindTo(Type dataType, string scope = null);

        /// <summary>
        /// Specifies the data scope. This will be used to identify the appropriate
        /// data providers
        /// </summary>
        /// <param name="type">The type of data to resolve at the page level</param>
        /// <param name="scopeName">The name of the data scope to use when resolving data providers</param>
        IPageDefinition DataScope(Type type, string scopeName);

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
        
        #endregion

        #region Non-visual components

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

        #endregion

        /// <summary>
        /// Builds the page
        /// </summary>
        IPage Build();
    }
}
