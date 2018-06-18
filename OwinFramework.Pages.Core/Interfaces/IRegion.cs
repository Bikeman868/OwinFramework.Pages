using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces
{
    /// <summary>
    /// A region is part of a layout. The region can contain a 
    /// single component or a layout
    /// </summary>
    public interface IRegion : IElement
    {
        /// <summary>
        /// Constructs an element that is the result of puttting the
        /// supplied element inside this region. The supplied element 
        /// should be either a component or a Layout.
        /// </summary>
        void Populate(IElement content);

        /// <summary>
        /// Constructs a new region instance that has a reference to
        /// the original region element, but can have different contents.
        /// The properties of this clone region will read/write to the
        /// original region definition, but the Populate() method will
        /// only populate the clone copy.
        /// This feature is necessary to allow different pages to contain
        /// the same regions but with different content on each page.
        /// </summary>
        /// <param name="content">The content to place inside the region.
        /// All other properties of the clone will reflect the original
        /// region that was cloned</param>
        IRegion Clone(IElement content);

        /// <summary>
        /// Returns a flag indicating if this is a clone or the original
        /// region definition. Calling the Populate() method on the original
        /// region will change the region contents for all pages that use
        /// this region and do not override the contents. Calling the 
        /// Populate() method on a clone region only affects that specific clone.
        /// </summary>
        bool IsClone { get; }

        /// <summary>
        /// Writes the html for this region with specific content inside
        /// </summary>
        /// <param name="renderContext">The context to render into</param>
        /// <param name="dataContext">The data to use for data binding operations</param>
        /// <param name="content">The element to render inside the region</param>
        IWriteResult WriteHtml(
            IRenderContext renderContext, 
            IDataContext dataContext, 
            IElement content);
    }
}
