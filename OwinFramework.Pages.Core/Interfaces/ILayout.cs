using System;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces
{
    /// <summary>
    /// Layouts define an arrangement of html containers that can contain components
    /// or other layouts. The layout emits css and JavaScript to make the layout
    /// perform its desired content layout
    /// </summary>
    public interface ILayout : IElement
    {
        /// <summary>
        /// Gets debugging information about the layout
        /// </summary>
        new DebugLayout GetDebugInfo();

        /// <summary>
        /// Changes the component that is displayed in a region of the layout
        /// </summary>
        /// <param name="regionName">The name of a region on this layout</param>
        /// <param name="element">The element to render in this region</param>
        void Populate(string regionName, IElement element);

        /// <summary>
        /// Gets a region from the layout
        /// </summary>
        IRegion GetRegion(string regionName);

        /// <summary>
        /// Constructs a new layout instance that has a reference to
        /// the original layout element, but can have different contents.
        /// The properties of this instance will read/write to the
        /// original layout definition, but the Populate() method will
        /// only populate the instance.
        /// This feature is necessary to allow different pages to contain
        /// the same layouts but with different content on each page.
        /// </summary>
        ILayout CreateInstance();

        /// <summary>
        /// Returns a flag indicating if this is a [age instance or the original
        /// layout definition. Calling the Populate() method on the original
        /// layout will change the region contents for all pages that use
        /// this layout and do not override the contents. Calling the 
        /// Populate() method on an instance only affects that specific instance.
        /// </summary>
        bool IsInstance { get; }

        /// <summary>
        /// Writes the html for this region with specific content inside
        /// </summary>
        /// <param name="context">The context to render into</param>
        /// <param name="contentFunc">A function that will return the region to render
        /// for each region name. This function should return null to render the default
        /// region for the layout</param>
        IWriteResult WriteHtml(
            IRenderContext context, 
            Func<string, IRegion> contentFunc);
    }
}
