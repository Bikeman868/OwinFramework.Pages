using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.DataModel;

namespace OwinFramework.Pages.Core.Interfaces.Runtime
{
    /// <summary>
    /// An instance of this interface is passed to the Initialize routine
    /// It is used to gather up all the informatrion needed to get the page ready
    /// </summary>
    public interface IPageData
    {
        /// <summary>
        /// Starts adding an element to the page. After calling this
        /// method add all of the children then call the EndAddElement
        /// method.
        /// </summary>
        /// <returns>The data context builder that will build the data
        /// context for children of this element</returns>
        IDataContextBuilder BeginAddElement(IElement element);

        /// <summary>
        /// This must be called exactly once for each call to BeginAddElement
        /// </summary>
        void EndAddElement(IElement element);

        /// <summary>
        /// Logs an initialization message that can be used to track down
        /// configuration issues.
        /// </summary>
        void Log(string message);
    }
}
