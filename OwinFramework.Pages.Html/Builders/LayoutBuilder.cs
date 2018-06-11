using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Builders
{
    internal class LayoutBuilder : ILayoutBuilder
    {
        private readonly INameManager _nameManager;

        public LayoutBuilder(
                INameManager nameManager)
        {
            _nameManager = nameManager;
        }

        ILayoutDefinition ILayoutBuilder.Layout()
        {
            return new LayoutDefinition(_nameManager);
        }

        private class LayoutDefinition: ILayoutDefinition
        {
            private readonly INameManager _nameManager;
            private readonly BuiltLayout _layout;

            private RegionSet _regionNames;
            private Dictionary<string, object> _regions;
            private Dictionary<string, object> _regionLayouts;
            private Dictionary<string, object> _regionComponents;

            private class RegionSet
            {
                public List<object> Elements;

                public static RegionSet Parse(string regions, ref int position)
                {
                    var result = new RegionSet
                        {
                            Elements = new List<object>()
                        };

                    var start = position;
                    while (position < regions.Length)
                    {
                        switch (regions[position])
                        {
                            case '(':
                                position++;
                                Parse(regions, ref position);
                                break;
                            case ')':
                                result.Elements = BuildList(regions, start, position);
                                return result;
                            default:
                                position++;
                                break;
                        }
                    }
                    result.Elements = BuildList(regions, start, position);
                    return result;
                }

                private static List<object> BuildList(string regions, int start, int end)
                {
                    var result = new List<object>();
                    if (end > start)
                    {
                        var regionNames = regions.Substring(start, end - start);
                        result.AddRange(regionNames.Split(',').Select(s => s.Trim()));
                    }
                    return result;
                }
            }

            public LayoutDefinition(
                INameManager nameManager)
            {
                _nameManager = nameManager;
                _layout = new BuiltLayout();
                _regions = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                _regionLayouts = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                _regionComponents = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            }

            public ILayoutDefinition Name(string name)
            {
                _layout.Name = name;
                return this;
            }

            public ILayoutDefinition PartOf(IPackage package)
            {
                _layout.Package = package;
                return this;
            }

            public ILayoutDefinition PartOf(string packageName)
            {
                _layout.Package = _nameManager.ResolvePackage(packageName);

                if (_layout.Package == null)
                    throw new LayoutBuilderException(
                        "Package names must be registered before layouts can refer to them. " +
                        "There is no registered package '" + packageName + "'");
                return this;
            }

            public ILayoutDefinition RegionNesting(string regionNesting)
            {
                var position = 0;
                _regionNames = RegionSet.Parse(regionNesting, ref position);
                return this;
            }

            public ILayoutDefinition AssetDeployment(AssetDeployment assetDeployment)
            {
                _layout.AssetDeployment = assetDeployment;
                return this;
            }

            public ILayoutDefinition Region(string regionName, IRegion region)
            {
                _regions[regionName] = region;
                return this;
            }

            public ILayoutDefinition Region(string regionName, string name)
            {
                _regions[regionName] = name;
                return this;
            }

            public ILayoutDefinition Component(string regionName, IComponent component)
            {
                _regionComponents[regionName] = component;
                return this;
            }

            public ILayoutDefinition Component(string regionName, string componentName)
            {
                _regionComponents[regionName] = componentName;
                return this;
            }

            public ILayoutDefinition Layout(string regionName, ILayout layout)
            {
                _regionLayouts[regionName] = layout;
                return this;
            }

            public ILayoutDefinition Layout(string regionName, string layoutName)
            {
                _regionLayouts[regionName] = layoutName;
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

            ILayoutDefinition ILayoutDefinition.BindTo<T>()
            {
                return this;
            }

            ILayoutDefinition ILayoutDefinition.BindTo(Type dataType)
            {
                return this;
            }

            ILayoutDefinition ILayoutDefinition.DataContext(string dataContextName)
            {
                return this;
            }

            public ILayout Build()
            {
                _nameManager.Register(_layout);
                _nameManager.AddResolutionHandler(() =>
                    {
                        _layout.Elements = new List<IElement>();
                    });
                return _layout;
            }
        }

        private class RegionContainer: Element
        {

        }

        private class BuiltLayout: Layout
        {
            public AssetDeployment AssetDeployment { get; set; }

            public List<IElement> Elements;
        }
    }
}
