using System;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Html.Elements;

namespace OwinFramework.Pages.Html.Builders
{
    internal class ModuleDefinition : IModuleDefinition
    {
        private readonly Type _declaringType;
        private readonly IFluentBuilder _fluentBuilder;
        private readonly BuiltModule _module;

        public ModuleDefinition(
            Type declaringType,
            IModuleDependenciesFactory moduleDependencies,
            IFluentBuilder fluentBuilder)
        {
            _declaringType = declaringType;
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

        public IModule Build()
        {
            return _fluentBuilder.Register(_module, _declaringType);
        }
    }
}
