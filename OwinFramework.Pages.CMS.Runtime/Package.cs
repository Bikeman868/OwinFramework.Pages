using System.Collections.Generic;
using Ioc.Modules;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using Prius.Contracts.Interfaces;
using OwinFramework.Pages.CMS.Runtime.Interfaces;
using OwinFramework.Pages.CMS.Runtime.Data;

namespace OwinFramework.Pages.CMS.Runtime
{
    /// <summary>
    /// Defines the IoC needs of this assembly
    /// </summary>
    [Ioc.Modules.Package]
    public class Package: Ioc.Modules.IPackage
    {
        string Ioc.Modules.IPackage.Name { get { return "Owin Framework Pages CMS runtime"; } }

        IList<IocRegistration> Ioc.Modules.IPackage.IocRegistrations
        {
            get 
            {
                return new List<IocRegistration>
                {
                    // These are internal classes that need wiring up with IoC
                    new IocRegistration().Init<IDatabaseReader, TestDatabaseReader>(),

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

                    new IocRegistration().Init<ICommandFactory>(),
                    new IocRegistration().Init<IContextFactory>(),
                };
            }
        }

    }
}
