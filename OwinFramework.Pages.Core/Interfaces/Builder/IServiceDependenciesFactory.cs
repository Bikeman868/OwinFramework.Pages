﻿using System;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.Interfaces.Builder
{
    /// <summary>
    /// The IoC dependencies are wrapped in this factory so that when
    /// new dependencies are added it does not change the constructor
    /// which would break any application code that inherits from it
    /// </summary>
    public interface IServiceDependenciesFactory
    {
        /// <summary>
        /// Constructs and initializes a page dependencies instance
        /// specific to the request
        /// </summary>
        IServiceDependencies Create(IOwinContext context, Action<IOwinContext, Func<string>> trace);

        /// <summary>
        /// The name manager is a singleton and therefore alwaya available
        /// </summary>
        INameManager NameManager { get; }

        /// <summary>
        /// The asset manager is a singleton and therefore alwaya available
        /// </summary>
        IAssetManager AssetManager { get; }

        /// <summary>
        /// The request router is a singelton and is therefore always available
        /// </summary>
        IRequestRouter RequestRouter { get; }

        /// <summary>
        /// A factory for constructing data consumers
        /// </summary>
        IDataConsumerFactory DataConsumerFactory { get; }

        /// <summary>
        /// A singelton that can find data suppliers
        /// </summary>
        IDataCatalog DataCatalog { get; }

        /// <summary>
        /// A factory for creating instances that capture a dependency on data
        /// </summary>
        IDataDependencyFactory DataDependencyFactory { get; }
    }
}
