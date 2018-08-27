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
        /// <param name="dataContextBuilder">The object that built the data context. This
        /// is required here in case the adat context is missing some data, in this case
        /// the IDataContextBuilder can add the missing data and remember this for the
        /// next time this runable is rendered</param>
        /// <param name="pageArea">The area of the page that is being written to</param>
        IWriteResult WritePageArea(
            IRenderContext context,
            IDataContextBuilder dataContextBuilder,
            PageArea pageArea);
    }
}
