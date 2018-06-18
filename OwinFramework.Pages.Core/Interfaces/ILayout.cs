using System;
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
        void Populate(string regionName, IElement element);

        /// <summary>
        /// Constructs a new laqyout instance that has a reference to
        /// the original layout element, but can have different contents.
        /// The properties of this clone layout will read/write to the
        /// original layout definition, but the Populate() method will
        /// only populate the clone copy.
        /// This feature is necessary to allow different pages to contain
        /// the same layouts but with different content on each page.
        /// </summary>
        ILayout Clone();

        /// <summary>
        /// Returns a flag indicating if this is a clone or the original
        /// layout definition. Calling the Populate() method on the original
        /// layout will change the region contents for all pages that use
        /// this layout and do not override the contents. Calling the 
        /// Populate() method on a clone layout only affects that specific clone.
        /// </summary>
        bool IsClone { get; }

        /// <summary>
        /// Writes the html for this region with specific content inside
        /// </summary>
        /// <param name="renderContext">The context to render into</param>
        /// <param name="dataContext">The data to use for data binding operations</param>
        /// <param name="contentFunc">A function that will return the element to render
        /// for each region. This function should return null to render the default
        /// region contents</param>
        IWriteResult WriteHtml(
            IRenderContext renderContext, 
            IDataContext dataContext, 
            Func<string, IElement> contentFunc);
    }
}
