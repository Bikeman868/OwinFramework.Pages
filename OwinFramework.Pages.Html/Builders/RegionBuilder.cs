using System;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Builders
{
    internal class RegionBuilder: IRegionBuilder
    {
        IRegionDefinition IRegionBuilder.Region()
        {
            return new RegionDefinition();
        }

        private class RegionDefinition: IRegionDefinition
        {
            public IRegionDefinition Name(string name)
            {
                return this;
            }

            public IRegionDefinition PartOf(Core.Interfaces.IPackage package)
            {
                return this;
            }

            public IRegionDefinition PartOf(string packageName)
            {
                return this;
            }

            public IRegionDefinition AssetDeployment(Core.Enums.AssetDeployment assetDeployment)
            {
                return this;
            }

            public IRegionDefinition Layout(Core.Interfaces.ILayout layout)
            {
                return this;
            }

            public IRegionDefinition Layout(string name)
            {
                return this;
            }

            public IRegionDefinition Component(Core.Interfaces.IComponent component)
            {
                return this;
            }

            public IRegionDefinition Component(string componentName)
            {
                return this;
            }

            public IRegionDefinition Tag(string tagName)
            {
                return this;
            }

            public IRegionDefinition ClassNames(params string[] classNames)
            {
                return this;
            }

            public IRegionDefinition Style(string style)
            {
                return this;
            }

            public IRegionDefinition ForEach<T>()
            {
                return this;
            }

            public IRegionDefinition BindTo<T>() where T : class
            {
                return this;
            }

            public IRegionDefinition BindTo(Type dataType)
            {
                return this;
            }

            public IRegionDefinition DataContext(string dataContextName)
            {
                return this;
            }

            public IRegionDefinition ForEach(Type dataType, string tag = "", string style = "", params string[] classes)
            {
                return this;
            }

            public Core.Interfaces.IRegion Build()
            {
                return new BuiltRegion();
            }
        }

        private class BuiltRegion: Region
        {
        }
    }
}
