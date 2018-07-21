using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces.DataModel
{
    /// <summary>
    /// Supplies a very specific type of data when executed
    /// </summary>
    public interface IDataSupply
    {
        /// <summary>
        /// The runtime will call this for each page request that needs the
        /// type of data suplied by this instance.
        /// </summary>
        /// <param name="renderContext">The request that is being handled</param>
        /// <param name="dataContext">The data context to use to get dependant data and where
        /// new data should be added</param>
        void Supply(IRenderContext renderContext, IDataContext dataContext);

        /// <summary>
        /// There are a few unusual situations where other objects need to
        /// know when data has been updated in the data context. For the most
        /// part this is not required because the data context is established
        /// then used for rendering operations.
        /// One case where this event is needed is when a region repeats its
        /// contents based on a list of objects, and one of the descendants
        /// of the region contents is bound to data that is derrived from the
        /// repeating type. Like I said this is a very rare and unusual situation.
        /// </summary>
        event EventHandler<DataSuppliedEventArgs> OnDataSupplied;
    }

    /// <summary>
    /// This encapsulates the arguments that are passed to the OnDataSupplied event
    /// </summary>
    public class DataSuppliedEventArgs : EventArgs
    {
        /// <summary>
        /// The render context that was updated
        /// </summary>
        public IRenderContext RenderContext { get; set; }

        /// <summary>
        /// The data context that was updated
        /// </summary>
        public IDataContext DataContext { get; set; }
    }
}
