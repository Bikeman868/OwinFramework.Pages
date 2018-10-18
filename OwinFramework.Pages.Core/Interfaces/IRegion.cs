using System;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces
{
    /// <summary>
    /// A region is part of a layout. The region can contain a 
    /// single component or a layout
    /// </summary>
    public interface IRegion : IElement, IDataRepeater
    {
        /// <summary>
        /// Retrieves the contents of this region
        /// </summary>
        IElement Content { get; set; }

        /// <summary>
        /// This is called for each area of the page where this instance wants
        /// to write content. This method is called if either the GetPageAreas() function
        /// returns true for the page area, or any of the children return true for this.
        /// </summary>
        /// <param name="context">The rendering operation in progress</param>
        /// <param name="pageArea">The area of the page that is being written to</param>
        /// <param name="onListItem">This action is invoked for each item in the list
        /// when the region is configured to repeat</param>
        /// <param name="contentWriter">A function that writes a contents of the region</param>
        IWriteResult WritePageArea(
            IRenderContext context,
            PageArea pageArea,
            Action<IRenderContext, object> onListItem,
            Func<IRenderContext, PageArea, IWriteResult> contentWriter);
    }
}
