using System;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Builders
{
    internal class ModuleBuilder: IModuleBuilder
    {
        IModuleDefinition IModuleBuilder.Module()
        {
            return new ModuleDefinition();
        }

        private class ModuleDefinition: IModuleDefinition
        {
            public IModuleDefinition Name(string name)
            {
                return this;
            }

            public IModuleDefinition AssetDeployment(Core.Enums.AssetDeployment assetDeployment)
            {
                return this;
            }

            public IModule Build()
            {
                return new BuiltModule();
            }
        }

        private class BuiltModule: Module
        {
        }
    }
}
