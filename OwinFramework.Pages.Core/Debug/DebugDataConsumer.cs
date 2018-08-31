using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwinFramework.Pages.Core.Debug
{
    /// <summary>
    /// This is used to communicate debugging information about a data consumer need
    /// </summary>
    public class DebugDataConsumer: DebugInfo
    {
        /// <summary>
        /// Returns a list of the supplies that this consumer needs
        /// </summary>
        public List<DebugDataSupply> DependentSupplies { get; set; }

        /// <summary>
        /// Returns a list of the supplies that this consumer needs
        /// </summary>
        public List<DebugDataScope> DependentData { get; set; }

        /// <summary>
        /// Returns a list of the providers that this consumer needs
        /// </summary>
        public List<DebugDataProviderDependency> DependentProviders { get; set; }

        /// <summary>
        /// Returns the default description of this consumer
        /// </summary>
        public override string ToString()
        {
            var description = new List<string>();

            if (DependentProviders != null)
                foreach (var dataProvider in DependentProviders)
                    description.Add("Consumes " + dataProvider);

            if (DependentSupplies != null)
                foreach (var dataSupply in DependentSupplies)
                    description.Add("Consumes " + dataSupply);

            if (DependentData != null)
                foreach (var dependency in DependentData)
                    description.Add("Consumes " + dependency);

            return string.Join(Environment.NewLine, description);
        }
    }
}
