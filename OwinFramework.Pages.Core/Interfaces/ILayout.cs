using System;
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
        /// <param name="regionName">The name of a region on this layout</param>
        /// <param name="element">The element to render in this region</param>
        void PopulateElement(string regionName, IElement element);

        /// <summary>
        /// Gets a region from the layout
        /// </summary>
        IRegion GetRegion(string regionName);

        /// <summary>
        /// This is called for each area of the page where this instance wants
        /// to write content. This method is called if either the GetPageAreas() function
        /// returns true for the page area, or any of the children retuen true for this.
        /// </summary>
        /// <param name="context">The rendering operation in progress</param>
        /// <param name="dataContextBuilder">The object that built the data context. This
        /// is required here in case the adat context is missing some data, in this case
        /// the IDataContextBuilder can add the missing data and remember this for the
        /// next time this runable is rendered</param>
        /// <param name="pageArea">The area of the page that is being written to</param>
        /// <param name="regionWriter">A function that writes a region of the layout. 
        /// The last string parameter is the name of the region to write</param>
        IWriteResult WritePageArea(
            IRenderContext context,
            IDataContextBuilder dataContextBuilder,
            PageArea pageArea,
            Func<IRenderContext, IDataContextBuilder, PageArea, string, IWriteResult> regionWriter);
    }
}
