using OwinFramework.Pages.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
