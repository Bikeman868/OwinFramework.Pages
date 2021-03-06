﻿using System.Collections.Generic;
using Ioc.Modules;
using OwinFramework.Interfaces.Builder;
using OwinFramework.Interfaces.Utility;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Core.Interfaces.Segmentation;

namespace Sample1
{
    /// <summary>
    /// Defines the IoC needs of this assembly
    /// </summary>
    [Package]
    public class Package: IPackage
    {
        string IPackage.Name { get { return "Sample1 website"; } }

        IList<IocRegistration> IPackage.IocRegistrations
        {
            get 
            {
                return new List<IocRegistration>
                {
                    new IocRegistration().Init<IRequestRouter>(),
                    new IocRegistration().Init<IHostingEnvironment>(),
                    new IocRegistration().Init<IConfiguration>(),
                    new IocRegistration().Init<IBuilder>(),
                    new IocRegistration().Init<IRequestRouter>(),
                    new IocRegistration().Init<IFluentBuilder>(),

                    new IocRegistration().Init<Prius.Contracts.Interfaces.External.IFactory, PriusIntegration.PriusFactory>(),
                    new IocRegistration().Init<Prius.Contracts.Interfaces.External.IErrorReporter, PriusIntegration.PriusErrorReporter>(),

                    new IocRegistration().Init<IUserSegmenter, UserSegmenter>(),
                };
            }
        }

    }
}
