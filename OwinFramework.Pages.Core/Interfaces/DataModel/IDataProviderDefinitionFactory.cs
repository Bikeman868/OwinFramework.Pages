using System;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces.DataModel
{
    /// <summary>
    /// Factory for IDataProviderDefinition
    /// </summary>
    public interface IDataProviderDefinitionFactory
    {
        /// <summary>
        /// Constructs and initializes a new data provider definition
        /// </summary>
        IDataProviderDefinition Create(IDataProvider dataProvider, IDataDependency dependency = null);
    }
}
