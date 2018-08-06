using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OwinFramework.Pages.Core.Interfaces.DataModel;

namespace OwinFramework.Pages.Core.Debug
{
    /// <summary>
    /// This is used to communicate debugging information about a data supply
    /// </summary>
    public class DebugDataSupply
    {
        /// <summary>
        /// The supplier that provided this daat supply
        /// </summary>
        public DebugDataSupplier Supplier { get; set; }

        /// <summary>
        /// Returns the default description
        /// </summary>
        public override string ToString()
        {
            var description = "data supply";

            if (Supplier != null)
                description += " from " + Supplier;

            return description;
        }
    }
}
