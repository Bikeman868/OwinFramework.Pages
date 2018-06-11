using System;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Builders
{
    internal class ComponentBuilder : IComponentBuilder
    {
        IComponentDefinition IComponentBuilder.Component()
        {
            return new ComponentDefinition();
        }

        private class ComponentDefinition: IComponentDefinition
        {
            IComponentDefinition IComponentDefinition.Name(string name)
            {
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
                return new BuiltComponent();
            }
        }

        private class BuiltComponent: Component
        {

        }
    }
}
