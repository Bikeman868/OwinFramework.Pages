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
        /// The supplies that this one depends on. These other supplies must
        /// be executed before this one
        /// </summary>
        IList<IDataSupply> DependentSupplies { get; }
    }
}
