using System.Collections.Generic;
using Ioc.Modules;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Framework.Builders;
using OwinFramework.Pages.Framework.Configuration;
using OwinFramework.Pages.Framework.DataModel;
using OwinFramework.Pages.Framework.Interfaces;
using OwinFramework.Pages.Framework.Managers;
using OwinFramework.Pages.Framework.Runtime;

namespace OwinFramework.Pages.Framework
{
    /// <summary>
    /// Defines the IoC needs of this assembly
    /// </summary>
    [Ioc.Modules.Package]
    public class Package: Ioc.Modules.IPackage
    {
        string Ioc.Modules.IPackage.Name { get { return "Owin Framework Pages framework"; } }

        IList<IocRegistration> Ioc.Modules.IPackage.IocRegistrations
        {
            get 
            {
                return new List<IocRegistration>
                {
                    // These interface mappings are internal to this assembly and
                    // just need to be wired up by IoC
                    new IocRegistration().Init<IFrameworkConfiguration, FrameworkConfiguration>(),
                    new IocRegistration().Init<IElementConfiguror, ElementConfiguror>(),
                    new IocRegistration().Init<IPackageDependenciesFactory, PackageDependenciesFactory>(),
                    new IocRegistration().Init<IDataProviderDependenciesFactory, DataProviderDependenciesFactory>(),
                    new IocRegistration().Init<IPackageDependencies, PackageDependencies>(IocLifetime.MultiInstance),
                    new IocRegistration().Init<IDataProviderDependencies, DataProviderDependencies>(IocLifetime.MultiInstance),
                    new IocRegistration().Init<IUserSegmenter, UserSegmenter>(),

                    // These classes are the data model
                    new IocRegistration().Init<IDataCatalog, DataCatalog>(),
                    new IocRegistration().Init<IDataContextFactory, DataContextFactory>(),
                    new IocRegistration().Init<IDataDependencyFactory, DataDependencyFactory>(),
                    new IocRegistration().Init<IDataSupplierFactory, DataSupplierFactory>(),
                    new IocRegistration().Init<IDataScopeFactory, DataScopeFactory>(),
                    new IocRegistration().Init<IDataScopeProviderFactory, DataScopeProviderFactory>(),
                    new IocRegistration().Init<IDataConsumerFactory, DataConsumerFactory>(),
                    new IocRegistration().Init<IDataContextBuilderFactory, DataContextBuilderFactory>(),

                    // These classes implement core facilities. They are thread-safe singletons
                    new IocRegistration().Init<IFluentBuilder, FluentBuilder>(),
                    new IocRegistration().Init<IElementConfiguror, ElementConfiguror>(),
                    new IocRegistration().Init<IAssetManager, AssetManager>(),
                    new IocRegistration().Init<INameManager, NameManager>(),
                    new IocRegistration().Init<IIdManager, IdManager>(),

                    // The request router is the top level entry point and the only
                    // interface that the middleware depends on
                    new IocRegistration().Init<IRequestRouter, RequestRouter>(),
                    new IocRegistration().Init<IRequestRouterFactory, RequestRouterFactory>(),

                    // These are the external dependencies for this package
                    new IocRegistration().Init<IHtmlWriterFactory>(),
                    new IocRegistration().Init<ICssWriterFactory>(),
                    new IocRegistration().Init<IJavascriptWriterFactory>(),
                    new IocRegistration().Init<IStringBuilderFactory>(),
                    new IocRegistration().Init<IDictionaryFactory>(),

                    new IocRegistration().Init<IModuleDependenciesFactory>(),
                    new IocRegistration().Init<IPageDependenciesFactory>(),
                    new IocRegistration().Init<ILayoutDependenciesFactory>(),
                    new IocRegistration().Init<IRegionDependenciesFactory>(),
                    new IocRegistration().Init<IComponentDependenciesFactory>(),
                    new IocRegistration().Init<IDataProviderDependenciesFactory>(),
                };
            }
        }

    }
}
