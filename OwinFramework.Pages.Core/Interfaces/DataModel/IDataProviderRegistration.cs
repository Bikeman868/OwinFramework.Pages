using System;
using System.Collections.Generic;

namespace OwinFramework.Pages.Core.Interfaces.DataModel
{
    /// <summary>
    /// Contains information about a registered data provider
    /// </summary>
    public interface IDataProviderRegistration
    {
        /// <summary>
        /// Gets the registered data provider
        /// </summary>
        IDataProvider DataProvider { get; }

        /// <summary>
        /// Gets the relative priority of this data provider. This is
        /// used to select data providers when multiple providers can
        /// satisfy the dependency
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// The scopes that this provider can satisfy
        /// </summary>
        IList<string> Scopes { get; }

        /// <summary>
        /// The types that this provider can add to the data context
        /// </summary>
        IList<Type> Types { get; }

        /// <summary>
        /// The other data providers that this one is dependent on
        /// </summary>
        IList<IDataProvider> DependentProviders { get; }
    }
}