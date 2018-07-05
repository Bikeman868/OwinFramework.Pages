using System;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces
{
    /// <summary>
    /// A region is part of a layout. The region can contain a 
    /// single component or a layout
    /// </summary>
    public interface IRegion : IElement, IDataScopeProvider
    {
        /// <summary>
        /// Retrieves debugging information about the region
        /// </summary>
        new DebugRegion GetDebugInfo();

        /// <summary>
        /// Retrieves the contents of this region
        /// </summary>
        IElement Content { get; }

        /// <summary>
        /// The scope name used to resolve data references in the 
        /// repeated data
        /// </summary>
        string RepeatScope { get; set; }

        /// <summary>
        /// The type of data to repeat
        /// </summary>
        Type RepeatType { get; set; }

        /// <summary>
        /// The scope name to use when resolving the list from
        /// context
        /// </summary>
        string ListScope { get; set; }

        /// <summary>
        /// The type of list that the region will look for in the
        /// data context
        /// </summary>
        Type ListType { get; }

        /// <summary>
        /// Constructs an element that is the result of puttting the
        /// supplied element inside this region. The supplied element 
        /// should be either a component or a Layout.
        /// </summary>
        void Populate(IElement content);

        /// <summary>
        /// Constructs a new region instance that has a reference to
        /// the original region element, but can have different contents.
        /// The properties of this instance will read/write to the
        /// original region definition, but the Populate() method will
        /// only populate the instance.
        /// This feature is necessary to allow different pages to contain
        /// the same regions but with different content on each page.
        /// </summary>
        /// <param name="content">The content to place inside the region.
        /// All other properties of the instance will reflect the original
        /// region</param>
        IRegion CreateInstance(IElement content);

        /// <summary>
        /// Returns a flag indicating if this is an instance or the original
        /// region definition. Calling the Populate() method on the original
        /// region will change the region contents for all pages that use
        /// this region and do not override the contents. Calling the 
        /// Populate() method on an instance only affects that specific instance.
        /// </summary>
        bool IsInstance { get; }

        /// <summary>
        /// Writes the page head with a specific data scope
        /// </summary>
        IWriteResult WriteHead(
            IRenderContext context, 
            IDataScopeProvider scope,
            bool includeChildren);

        /// <summary>
        /// Writes the html for this region with specific content inside and
        /// a specific data scpoe
        /// </summary>
        IWriteResult WriteHtml(
            IRenderContext context, 
            IDataScopeProvider scope,
            IElement content);

        /// <summary>
        /// Writes the page initialization script with a specific data scope
        /// </summary>
        IWriteResult WriteInitializationScript(
            IRenderContext context,
            IDataScopeProvider scope,
            bool includeChildren);

        /// <summary>
        /// Writes the page title with a specific data scope
        /// </summary>
        IWriteResult WriteTitle(
            IRenderContext context,
            IDataScopeProvider scope,
            bool includeChildren);
    }
}
