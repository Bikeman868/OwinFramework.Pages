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
    /// perform its desired content layout. Zones within the layout can also have
    /// custom region definitions to give fine-grained control over the layout behavior.
    /// </summary>
    public interface ILayout : IElement
    {
        /// <summary>
        /// Changes the element that is displayed in a region of the layout
        /// </summary>
        /// <param name="zoneName">The name of a zone on this layout</param>
        /// <param name="element">The element to render in this zone</param>
        void SetZoneElement(string zoneName, IElement element);

        /// <summary>
        /// Gets region that controls the behavior of a zone within the layout
        /// </summary>
        IRegion GetRegion(string zoneName);

        /// <summary>
        /// Gets the element that will be written in the specified zone
        /// </summary>
        IElement GetElement(string zoneName);

        /// <summary>
        /// Returns a list of the zone names for this layout
        /// </summary>
        IEnumerable<string> GetZoneNames();

        /// <summary>
        /// This is called for each area of the page where this layout wants
        /// to write content. This method is called if either the GetPageAreas() function
        /// returns true for the page area, or any of the children return true for this.
        /// </summary>
        /// <param name="context">The rendering operation in progress</param>
        /// <param name="pageArea">The area of the page that is being written to</param>
        /// <param name="zoneWriter">A function that writes a zone of the layout. The 
        /// parameters to this function are:
        /// 1. The rendering context to write to
        /// 2. The area of the page that is being written
        /// 3. The name of the zone to write
        /// The last string parameter is the name of the zone to write</param>
        IWriteResult WritePageArea(
            IRenderContext context,
            PageArea pageArea,
            Func<IRenderContext, PageArea, string, IWriteResult> zoneWriter);
    }
}
