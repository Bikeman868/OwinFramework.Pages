using System;
using System.Collections.Generic;

namespace OwinFramework.Pages.Core.Interfaces.DataModel
{
    /// <summary>
    /// Defines the data needs of a data consumer
    /// </summary>
    public interface IDataConsumerNeeds
    {
        /// <summary>
        /// When a data consumer needs a specific kind of data from a specific provider
        /// these needs are expressed here. If the IDataDependency is null then the
        /// default data for this provider will be requested
        /// </summary>
        List<Tuple<IDataProvider, IDataDependency>> DataProviderDependencies { get; }

        /// <summary>
        /// When a data consumer depends on a specific data supply these needs are
        /// expressed here
        /// </summary>
        List<IDataSupply> DataSupplyDependencies { get; }

        /// <summary>
        /// This is the most usual use case where the data consumer needs a specific
        /// kind of data but does not care who provides it
        /// </summary>
        List<IDataDependency> DataDependencies { get; }
    }
}
