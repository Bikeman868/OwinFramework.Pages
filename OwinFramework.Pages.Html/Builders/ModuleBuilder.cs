using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;

namespace OwinFramework.Pages.Html.Builders
{
    internal class ModuleBuilder: IModuleBuilder
    {
        private readonly INameManager _nameManager;
        private readonly IModuleDependenciesFactory _moduleDependenciesFactory;

        public ModuleBuilder(
            IModuleDependenciesFactory moduleDependenciesFactory,
                INameManager nameManager)
        {
            _moduleDependenciesFactory = moduleDependenciesFactory;
            _nameManager = nameManager;
        }

        IModuleDefinition IModuleBuilder.Module()
        {
            return new ModuleDefinition(_moduleDependenciesFactory, _nameManager);
        }

    }
}
