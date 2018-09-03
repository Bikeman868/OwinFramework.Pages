using System;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces
{
    /// <summary>
    /// Components render html, css and JavaScript into the output
    /// </summary>
    public interface IComponent : IElement
    {
        /// <summary>
        /// This is called for each area of the page where this instance wants
        /// to write content. This method is called if either the GetPageAreas() function
        /// returns true for the page area, or any of the children retuen true for this.
        /// </summary>
        /// <param name="context">The rendering operation in progress</param>
        /// <param name="pageArea">The area of the page that is being written to</param>
        IWriteResult WritePageArea(
            IRenderContext context,
            PageArea pageArea);
    }
}
