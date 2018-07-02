using System;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;

namespace OwinFramework.Pages.Html.Builders
{
    internal class ModuleDefinition : IModuleDefinition
    {
        private readonly IFluentBuilder _fluentBuilder;
        private readonly BuiltModule _module;

        public ModuleDefinition(
            IModuleDependenciesFactory moduleDependencies,
            IFluentBuilder fluentBuilder)
        {
            _fluentBuilder = fluentBuilder;
            _module = new BuiltModule(moduleDependencies);
        }

        public IModuleDefinition Name(string name)
        {
            _module.Name = name;
            return this;
        }

        public IModuleDefinition AssetDeployment(AssetDeployment assetDeployment)
        {
            _module.AssetDeployment = assetDeployment;
            return this;
        }

        public IModule Build(Type type)
        {
            return _fluentBuilder.Register(_module, type);
        }
    }
}
