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
            public IComponentDefinition Name(string name)
            {
                return this;
            }

            public IComponentDefinition PartOf(Core.Interfaces.IPackage package)
            {
                return this;
            }

            public IComponentDefinition PartOf(string packageName)
            {
                return this;
            }

            public IComponentDefinition AssetDeployment(Core.Enums.AssetDeployment assetDeployment)
            {
                return this;
            }

            public IComponentDefinition BindTo<T>() where T : class
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
