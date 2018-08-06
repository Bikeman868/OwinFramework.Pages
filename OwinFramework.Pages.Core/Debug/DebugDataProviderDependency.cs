using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OwinFramework.Pages.Core.Interfaces.DataModel;

namespace OwinFramework.Pages.Core.Debug
{
    /// <summary>
    /// This is used to communicate debugging information about a data provider dependency
    /// </summary>
    public class DebugDataProviderDependency
    {
        /// <summary>
        /// The data provider that is needed
        /// </summary>
        public DebugDataProvider DataProvider { get; set; }

        /// <summary>
        /// The type of data to request from this provider
        /// </summary>
        public DebugDataScope Data { get; set; }

        /// <summary>
        /// Returns the default description
        /// </summary>
        public override string ToString()
        {
            if (Data == null)
                return DataProvider.ToString();

            var description = Data.ToString();

            if (DataProvider != null)
                description += " from " + DataProvider;

            return description;
        }
    }
}
