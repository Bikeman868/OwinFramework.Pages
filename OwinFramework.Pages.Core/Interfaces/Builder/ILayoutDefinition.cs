using OwinFramework.Pages.Core.Enums;

namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// Fluent interface for defining layouts
    /// </summary>
    public interface ILayoutDefinition
    {
        /// <summary>
        /// Sets the name of the layout so that it can be referenced
        /// by name when configuring pages
        /// </summary>
        ILayoutDefinition Name(string name);

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
        /// Builds the layout
        /// </summary>
        ILayout Build();
    }
}
