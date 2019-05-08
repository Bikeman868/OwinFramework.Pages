using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Enums;
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
        /// Changes the component that is displayed in a region of the layout
        /// </summary>
        /// <param name="zoneName">The name of a region on this layout</param>
        /// <param name="element">The element to render in this region</param>
        void PopulateElement(string zoneName, IElement element);

        /// <summary>
        /// Gets a region from the layout
        /// </summary>
        IRegion GetRegion(string zoneName);

        /// <summary>
        /// Gets the element that is within the specified region
        /// </summary>
        IElement GetElement(string zoneName);

        /// <summary>
        /// Returns a list of the regions on this layout
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetZoneNames();

        /// <summary>
        /// This is called for each area of the page where this instance wants
        /// to write content. This method is called if either the GetPageAreas() function
        /// returns true for the page area, or any of the children retuen true for this.
        /// </summary>
        /// <param name="context">The rendering operation in progress</param>
        /// <param name="pageArea">The area of the page that is being written to</param>
        /// <param name="regionWriter">A function that writes a region of the layout. 
        /// The last string parameter is the name of the region to write</param>
        IWriteResult WritePageArea(
            IRenderContext context,
            PageArea pageArea,
            Func<IRenderContext, PageArea, string, IWriteResult> regionWriter);
    }
}
