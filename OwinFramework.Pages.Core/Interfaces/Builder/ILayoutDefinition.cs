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
        /// Defines how regions are nested. By default they are one after the
        /// other in the HTML, but this can be changed by this method.
        /// Excample "r1,r2,r3(r4,r5)" specifies that r4 and r5 are inside r3
        /// </summary>
        ILayoutDefinition RegionNesting(string regionNesting);

        /// <summary>
        /// Overrides the default asset deployment scheme for this layout
        /// </summary>
        ILayoutDefinition AssetDeployment(AssetDeployment assetDeployment);

        /// <summary>
        /// Adds a region to a layout
        /// </summary>
        ILayoutDefinition Region(IRegion region, string regionName);

        /// <summary>
        /// Adds a named region to a layout
        /// </summary>
        ILayoutDefinition Region(string name, string regionName);

        /// <summary>
        /// Overrides the component to place in a region
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
        /// Builds the layout
        /// </summary>
        ILayout Build();
    }
}
