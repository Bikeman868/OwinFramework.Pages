using System;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;

namespace OwinFramework.Pages.Framework.Builders
{
    internal class PackageDefinition : IPackageDefinition
    {
        private readonly IFluentBuilder _builder;
        private readonly INameManager _nameManager;
        private readonly BuiltPackage _package;
        private readonly Type _declaringType;

        public PackageDefinition(
            Type declaringType,
            IFluentBuilder builder,
            INameManager nameManager)
        {
            _declaringType = declaringType;
            _builder = builder;
            _nameManager = nameManager;
            _package = new BuiltPackage();
        }

        IPackageDefinition IPackageDefinition.Name(string name)
        {
            _package.Name = name;
            return this;
        }

        IPackageDefinition IPackageDefinition.NamespaceName(string namespaceName)
        {
            _package.NamespaceName = namespaceName;
            return this;
        }

        IPackageDefinition IPackageDefinition.Module(string moduleName)
        {
            _nameManager.AddResolutionHandler(nm => _package.Module = nm.ResolveModule("moduleName"));
            return this;
        }

        IPackageDefinition IPackageDefinition.Module(IModule module)
        {
            _package.Module = module;
            return this;
        }

        IPackage IPackageDefinition.Build()
        {
            return _builder.Register(_package);
        }
    }
}
