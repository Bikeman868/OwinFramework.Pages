using OwinFramework.Pages.Core.Enums;

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
        /// <returns></returns>
        IRegionDefinition ForEach<T>();

        /// <summary>
        /// Builds the region
        /// </summary>
        IRegion Build();
    }
}
