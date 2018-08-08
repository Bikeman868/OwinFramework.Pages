using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;
using OwinFramework.Pages.Core.Interfaces.DataModel;

namespace OwinFramework.Pages.Core.Debug
{
    /// <summary>
    /// This is used to communicate debugging information about a data supply
    /// </summary>
    public class DebugDataSupply
    {
        /// <summary>
        /// The data supply that this is debug info for
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public IDataSupply Instance { get; set; }

        /// <summary>
        /// Static means that the data can be set up once for the request.
        /// Not static means that the data changes during the processing
        /// of the request (for example a data bound repeater).
        /// </summary>
        public bool IsStatic { get; set; }

        /// <summary>
        /// The number of supplies that are subscribed to changes in this
        /// data. Only applicable for non-static supplies.
        /// </summary>
        public int SubscriberCount { get; set; }

        /// <summary>
        /// The type of data that will be added to the data context
        /// </summary>
        public DebugDataScope SuppliedData { get; set; }

        /// <summary>
        /// The supplier that provided this daat supply
        /// </summary>
        public DebugDataSupplier Supplier { get; set; }

        /// <summary>
        /// Returns the default description
        /// </summary>
        public override string ToString()
        {
            var description = (IsStatic ? "static" : "dynamic") + " supply";

            if (SuppliedData != null)
                description += " of " + SuppliedData;

            if (Supplier != null)
                description += " sourced from " + Supplier;

            return description;
        }
    }
}
