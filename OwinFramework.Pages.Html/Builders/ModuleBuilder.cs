using System;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Builders
{
    /// <summary>
    /// Plug-in to the fluent builder for building modules.
    /// Uses the supplied Module or constructs a new Module instance.
    /// Returns a fluent interface for defining the module characteristics
    /// </summary>
    internal class ModuleBuilder : IModuleBuilder
    {
        private readonly IFluentBuilder _fluentBuilder;
        private readonly IModuleDependenciesFactory _moduleDependenciesFactory;
        private readonly IElementConfiguror _elementConfiguror;

        public ModuleBuilder(
            IModuleDependenciesFactory moduleDependenciesFactory,
            IElementConfiguror elementConfiguror,
            IFluentBuilder fluentBuilder)
        {
            _moduleDependenciesFactory = moduleDependenciesFactory;
            _elementConfiguror = elementConfiguror;
            _fluentBuilder = fluentBuilder;
        }

        IModuleDefinition IModuleBuilder.BuildUpModule(object moduleInstance, Type declaringType)
        {
            var module = moduleInstance as Module ?? new Module(_moduleDependenciesFactory);
            if (declaringType == null) declaringType = (moduleInstance ?? module).GetType();

            var attributes = new AttributeSet(declaringType);
            _elementConfiguror.Configure(module, attributes);

            var moduleDefinition = new ModuleDefinition(module, _fluentBuilder);
            Configure(moduleDefinition, attributes);

            return moduleDefinition;
        }

        private void Configure(IModuleDefinition moduleDefinition, AttributeSet attributes)
        {
            // No additional configuration options for the concrete Module class
        }

    }
}