using System;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Builders
{
    internal class ModuleDefinition : IModuleDefinition
    {
        private readonly IFluentBuilder _fluentBuilder;
        private readonly Module _module;

        public ModuleDefinition(
            Module module,
            IFluentBuilder fluentBuilder)
        {
            _module = module;
            _fluentBuilder = fluentBuilder;
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
            _fluentBuilder.Register(_module);
            return _module;
        }
    }
}
