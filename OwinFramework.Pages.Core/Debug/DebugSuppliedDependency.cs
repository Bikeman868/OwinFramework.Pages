using System.Collections.Generic;

namespace OwinFramework.Pages.Core.Debug
{
    /// <summary>
    /// This is used to communicate debugging information about the 
    /// data supplied by a data provider
    /// </summary>
    public class DebugSuppliedDependency : DebugInfo
    {
        /// <summary>
        /// The supplier that provided this data
        /// </summary>
        public DebugDataSupplier Supplier { get; set; }

        /// <summary>
        /// The type of data that will be supplied
        /// </summary>
        public DebugDataScope DataTypeSupplied { get; set; }

        /// <summary>
        /// The supplies that must run before this one
        /// </summary>
        public List<DebugDataSupply> DependentSupplies { get; set; }

        /// <summary>
        /// Returns the default description
        /// </summary>
        public override string ToString()
        {
            if (DataTypeSupplied != null)
            {
                if (Supplier != null)
                    return "supply " + DataTypeSupplied + " sourced from " + Supplier;
                return "supply " + DataTypeSupplied;
            }

            return "supplied dependency";
        }
    }
}
