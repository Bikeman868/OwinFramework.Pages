using System;
using System.Collections.Generic;

namespace OwinFramework.Pages.Core.Interfaces.DataModel
{
    /// <summary>
    /// Defines the data needs of a data consumer
    /// </summary>
    public interface IDataConsumerNeeds
    {
        List<Tuple<IDataProvider, IDataDependency>> DataProviderDependencies { get; }
        List<IDataSupply> DataSupplyDependencies;
        List<IDataDependency> DataDependencies;
    }
}
