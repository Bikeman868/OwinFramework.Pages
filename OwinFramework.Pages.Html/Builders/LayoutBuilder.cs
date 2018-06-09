using System;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Builders
{
    internal class LayoutBuilder : ILayoutBuilder
    {
        ILayoutDefinition ILayoutBuilder.Layout()
        {
            return new LayoutDefinition();
        }

        private class LayoutDefinition: ILayoutDefinition
        {
            public ILayoutDefinition Name(string name)
            {
                return this;
            }

            public ILayoutDefinition PartOf(IPackage package)
            {
                return this;
            }

            public ILayoutDefinition PartOf(string packageName)
            {
                return this;
            }

            public ILayoutDefinition RegionNesting(string regionNesting)
            {
                return this;
            }

            public ILayoutDefinition AssetDeployment(Core.Enums.AssetDeployment assetDeployment)
            {
                return this;
            }

            public ILayoutDefinition Region(string regionName, Core.Interfaces.IRegion region)
            {
                return this;
            }

            public ILayoutDefinition Region(string regionName, string name)
            {
                return this;
            }

            public ILayoutDefinition Component(string regionName, Core.Interfaces.IComponent component)
            {
                return this;
            }

            public ILayoutDefinition Component(string regionName, string componentName)
            {
                return this;
            }

            public ILayoutDefinition Layout(string regionName, Core.Interfaces.ILayout layout)
            {
                return this;
            }

            public ILayoutDefinition Layout(string regionName, string layoutName)
            {
                return this;
            }

            public ILayoutDefinition Tag(string tagName)
            {
                return this;
            }

            public ILayoutDefinition ClassNames(params string[] classNames)
            {
                return this;
            }

            public ILayoutDefinition Style(string style)
            {
                return this;
            }

            public ILayoutDefinition NestingTag(string tagName)
            {
                return this;
            }

            public ILayoutDefinition NestedClassNames(params string[] classNames)
            {
                return this;
            }

            public ILayoutDefinition NestedStyle(string style)
            {
                return this;
            }

            public ILayout Build()
            {
                return new BuiltLayout();
            }
        }

        private class BuiltLayout: Layout
        {
        }
    }
}
