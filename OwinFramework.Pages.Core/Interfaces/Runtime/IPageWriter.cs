using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces
{
    /// <summary>
    /// Defines classes that write HTML onto the page
    /// </summary>
    public interface IPageWriter
    {
        /// <summary>
        /// Returns a list of the areas of the page that this element writes to
        /// </summary>
        IEnumerable<PageArea> GetPageAreas();

        /// <summary>
        /// This is where the element is responsible for outputting its dynamic css rules.
        /// Dynamic assets are defined as assets whose names are randomly
        /// generated at runtime and are therefore different on each web server.
        /// These types of asset are always rendered into the page.
        /// The recommended approach is to generate these names using the INameManager
        /// in the constructor of your element and keep using the same names
        /// for the lifetime of the instance.
        /// This method might be called in the context of rendering a page, or it might
        /// be called once for the page and the output cached for reuse on all subsequent
        /// pages.
        /// </summary>
        /// <param name="writer">A mechanism for writing correctly formatted CSS</param>
        /// <param name="childrenWriter">A function that writes the children. Can be null
        /// if there are no children. Normally this function would recursively call
        /// this method for each of the children.</param>
        IWriteResult WriteInPageStyles(
            ICssWriter writer, 
            Func<ICssWriter, IWriteResult, IWriteResult> childrenWriter);

        /// <summary>
        /// This is where the element is responsible for outputting its dynamic Javascript.
        /// Dynamic assets are defined as assets whose names are randomly
        /// generated at runtime and are therefore different on each web server.
        /// These types of asset are always rendered into the page.
        /// The recommended approach is to generate these names using the INameManager
        /// in the constructor of your element and keep using the same names
        /// for the lifetime of the instance.
        /// This method might be called in the context of rendering a page, or it might
        /// be called once for the page and the output cached for reuse on all subsequent
        /// pages.
        /// </summary>
        /// <param name="writer">A mechanism for writing correctly formatted JavaScript</param>
        /// <param name="childrenWriter">A function that writes the children. Can be null
        /// if there are no children. Normally this function would recursively call
        /// this method for each of the children.</param>
        IWriteResult WriteInPageFunctions(
            IJavascriptWriter writer, 
            Func<IJavascriptWriter, IWriteResult, IWriteResult> childrenWriter);
    }
}
