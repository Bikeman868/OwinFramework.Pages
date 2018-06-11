using System;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Builders
{
    internal class ComponentBuilder : IComponentBuilder
    {
        private readonly INameManager _nameManager;

        public ComponentBuilder(
                INameManager nameManager)
        {
            _nameManager = nameManager;
        }

        IComponentDefinition IComponentBuilder.Component()
        {
            return new ComponentDefinition(_nameManager);
        }

        private class ComponentDefinition: IComponentDefinition
        {
            private readonly INameManager _nameManager;
            private readonly BuiltComponent _component;

            public ComponentDefinition(
                INameManager nameManager)
            {
                _nameManager = nameManager;
                _component = new BuiltComponent();
            }

            IComponentDefinition IComponentDefinition.Name(string name)
            {
                _component.Name = name;
                return this;
            }

            IComponentDefinition IComponentDefinition.PartOf(IPackage package)
            {
                return this;
            }

            IComponentDefinition IComponentDefinition.PartOf(string packageName)
            {
                return this;
            }

            IComponentDefinition IComponentDefinition.AssetDeployment(Core.Enums.AssetDeployment assetDeployment)
            {
                return this;
            }

            IComponentDefinition IComponentDefinition.BindTo<T>()
            {
                return this;
            }

            IComponentDefinition IComponentDefinition.BindTo(Type dataType)
            {
                return this;
            }

            IComponentDefinition IComponentDefinition.DataContext(string dataContextName)
            {
                return this;
            }

            public IComponent Build()
            {
                _nameManager.Register(_component);
                return _component;
            }
        }

        private class BuiltComponent: Component
        {

        }
    }
}
