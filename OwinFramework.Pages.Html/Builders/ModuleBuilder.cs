using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Builders
{
    internal class ModuleBuilder: IModuleBuilder
    {
        private readonly INameManager _nameManager;

        public ModuleBuilder(
                INameManager nameManager)
        {
            _nameManager = nameManager;
        }

        IModuleDefinition IModuleBuilder.Module()
        {
            return new ModuleDefinition(_nameManager);
        }

        private class ModuleDefinition: IModuleDefinition
        {
            private readonly INameManager _nameManager;
            private readonly BuiltModule _module;

            public ModuleDefinition(
                INameManager nameManager)
            {
                _nameManager = nameManager;
                _module = new BuiltModule();
            }

            public IModuleDefinition Name(string name)
            {
                _module.Name = name;
                return this;
            }

            public IModuleDefinition AssetDeployment(AssetDeployment assetDeployment)
            {
                return this;
            }

            public IModule Build()
            {
                _nameManager.Register(_module);
                return _module;
            }
        }
    }
}
