using System;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;

namespace OwinFramework.Pages.Framework.Builders
{
    internal class PackageDefinition : IPackageDefinition
    {
        private readonly IFluentBuilder _builder;
        private readonly INameManager _nameManager;
        private readonly IPackage _package;

        public PackageDefinition(
            IPackage package,
            IFluentBuilder builder,
            INameManager nameManager)
        {
            _package = package;
            _builder = builder;
            _nameManager = nameManager;
        }

        IPackageDefinition IPackageDefinition.Name(string name)
        {
            if (!string.IsNullOrEmpty(name))
                _package.Name = name;
            return this;
        }

        IPackageDefinition IPackageDefinition.NamespaceName(string namespaceName)
        {
            if (!string.IsNullOrEmpty(namespaceName))
                _package.NamespaceName = namespaceName;
            return this;
        }

        IPackageDefinition IPackageDefinition.Module(string moduleName)
        {
            if (string.IsNullOrEmpty(moduleName)) return this;
                
            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolveElementReferences,
                (nm, p, n) => p.Module = nm.ResolveModule(n),
                _package,
                moduleName);

            return this;
        }

        IPackageDefinition IPackageDefinition.Module(IModule module)
        {
            _package.Module = module;
            return this;
        }

        IPackage IPackageDefinition.Build()
        {
            _builder.Register(_package);
            return _package;
        }
    }
}
