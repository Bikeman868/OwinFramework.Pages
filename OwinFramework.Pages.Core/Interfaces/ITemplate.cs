using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces
{
    /// <summary>
    /// Templates are parsed from documents that describe fragmens of Html.
    /// Templates can reference layouts, regions and components.
    /// Templates can use data-binding expressions wihtin the Html.
    /// </summary>
    public interface ITemplate : IPageWriter, IPackagable, INamed
    {
        /// <summary>
        /// This is called once during the rendering of the page body.
        /// Templates can not write to the page head or any other area of the page.
        /// </summary>
        /// <param name="context">The rendering operation in progress</param>
        /// <param name="pageArea">The area of the page to write to</param>
        IWriteResult WritePageArea(IRenderContext context, PageArea pageArea);
    }
}
