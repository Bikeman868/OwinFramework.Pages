using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Extensions;

namespace OwinFramework.Pages.Core.Debug
{
    /// <summary>
    /// This is used to communicate debugging information about a data supplier
    /// </summary>
    public class DebugDataSupplier : DebugInfo
    {
        /// <summary>
        /// Returns a list of the types of data that this supplier can supply
        /// </summary>
        public List<Type> SuppliedTypes { get; set; }

        /// <summary>
        /// The type of data that will be supplied if no data type is specified
        /// </summary>
        public DebugDataScope DefaultSupply { get; set; }

        /// <summary>
        /// Returns the default description
        /// </summary>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Name))
                return Name.ToLower();

            if (SuppliedTypes != null && SuppliedTypes.Count > 0)
            {
                return 
                    "supplier of [" + 
                    string.Join(", ", SuppliedTypes.Select(t => t.DisplayName(TypeExtensions.NamespaceOption.None))) +
                    "]";
            }

            if (DefaultSupply != null)
            {
                return "supplier of " + DefaultSupply;
            }

            return "data supplier";
        }
    }
}
