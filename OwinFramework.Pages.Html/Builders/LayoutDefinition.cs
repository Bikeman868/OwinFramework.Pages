using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Html.Interfaces;

namespace OwinFramework.Pages.Html.Builders
{
    public class LayoutDefinition : ILayoutDefinition
    {
        private readonly INameManager _nameManager;
        private readonly BuiltLayout _layout;
        private readonly IHtmlHelper _htmlHelper;

        private RegionSet _regionSet;
        private readonly Dictionary<string, object> _regionElements;
        private readonly Dictionary<string, object> _regionLayouts;
        private readonly Dictionary<string, object> _regionComponents;

        private string _tag;
        private string[] _classNames;
        private string _style;

        private string _nestingTag = "div";
        private string[] _nestedClassNames;
        private string _nestedStyle;

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
                            result.Elements.AddRange(BuildList(regions, start, position));
                            position++;
                            result.Elements.Add(Parse(regions, ref position));
                            position++;
                            start = position;
                            break;
                        case ')':
                            result.Elements.AddRange(BuildList(regions, start, position));
                            return result;
                        default:
                            position++;
                            break;
                    }
                }
                result.Elements.AddRange(BuildList(regions, start, position));
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
            INameManager nameManager,
            IHtmlHelper htmlHelper,
            ILayoutDependenciesFactory layoutDependenciesFactory,
            IPackage package)
        {
            _nameManager = nameManager;
            _htmlHelper = htmlHelper;

            _layout = new BuiltLayout(layoutDependenciesFactory);
            _regionElements = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            _regionLayouts = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            _regionComponents = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            _layout.Package = package;
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
            _regionSet = RegionSet.Parse(regionNesting, ref position);
            return this;
        }

        public ILayoutDefinition AssetDeployment(AssetDeployment assetDeployment)
        {
            _layout.AssetDeployment = assetDeployment;
            return this;
        }

        public ILayoutDefinition Region(string regionName, IRegion region)
        {
            _regionElements[regionName] = region;
            return this;
        }

        public ILayoutDefinition Region(string regionName, string name)
        {
            _regionElements[regionName] = name;
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
            _tag = tagName;
            return this;
        }

        public ILayoutDefinition ClassNames(params string[] classNames)
        {
            _classNames = classNames;
            return this;
        }

        public ILayoutDefinition Style(string style)
        {
            _style = style;
            return this;
        }

        public ILayoutDefinition NestingTag(string tagName)
        {
            _nestingTag = tagName;
            return this;
        }

        public ILayoutDefinition NestedClassNames(params string[] classNames)
        {
            _nestedClassNames = classNames;
            return this;
        }

        public ILayoutDefinition NestedStyle(string style)
        {
            _nestedStyle = style;
            return this;
        }

        ILayoutDefinition ILayoutDefinition.BindTo<T>(string scopeName)
        {
            return this;
        }

        ILayoutDefinition ILayoutDefinition.BindTo(Type dataType, string scopeName)
        {
            return this;
        }

        ILayoutDefinition ILayoutDefinition.DataProvider(string providerName)
        {
            return this;
        }

        ILayoutDefinition ILayoutDefinition.DataProvider(IDataProvider dataProvider)
        {
            // TODO: Data binding
            return this;
        }

        ILayoutDefinition ILayoutDefinition.DeployIn(IModule module)
        {
            _layout.Module = module;
            return this;
        }

        ILayoutDefinition ILayoutDefinition.DeployIn(string moduleName)
        {
            _nameManager.AddResolutionHandler(() =>
            {
                _layout.Module = _nameManager.ResolveModule(moduleName);
            });
            return this;
        }

        ILayoutDefinition ILayoutDefinition.NeedsComponent(string componentName)
        {
            _nameManager.AddResolutionHandler(() =>
            {
                _layout.NeedsComponent(_nameManager.ResolveComponent(componentName, _layout.Package));
            });
            return this;
        }

        ILayoutDefinition ILayoutDefinition.NeedsComponent(IComponent component)
        {
            _layout.NeedsComponent(component);
            return this;
        }

        public ILayout Build()
        {
            _nameManager.Register(_layout);
            _nameManager.AddResolutionHandler(() =>
            {
                var regionComponentKeys = _regionComponents.Keys.ToList();
                foreach (var regionName in regionComponentKeys)
                {
                    var componentRef = _regionComponents[regionName];
                    var componentName = componentRef as string;
                    var component = componentRef as IComponent;

                    if (componentName != null)
                        component = _nameManager.ResolveComponent(componentName, _layout.Package);

                    _regionComponents[regionName] = component;
                }

                var regionLayoutKeys = _regionLayouts.Keys.ToList();
                foreach (var regionName in regionLayoutKeys)
                {
                    var layoutRef = _regionLayouts[regionName];
                    var layoutName = layoutRef as string;
                    var layout = layoutRef as ILayout;

                    if (layoutName != null)
                        layout = _nameManager.ResolveLayout(layoutName, _layout.Package);

                    _regionLayouts[regionName] = layout;
                }

                WriteOpeningTag();
                AddRegionSet(_regionSet);
                WriteClosingTag();
            });
            return _layout;
        }

        private void AddRegionSet(RegionSet regionSet)
        {
            if (regionSet != null && regionSet.Elements != null)
            {
                foreach (var element in regionSet.Elements)
                {
                    var regionName = element as string;
                    var childRegionSet = element as RegionSet;

                    if (regionName != null)
                    {
                        if (!_regionElements.ContainsKey(regionName))
                        {
                            _layout.AddVisualElement(w => w.WriteElement("p", "Layout does not have a '" + regionName + "' region"), null);
                            continue;
                        }

                        var regionElement = _regionElements[regionName];
                        var region = regionElement as IRegion;
                        var regionElementName = regionElement as string;

                        if (region == null && regionElementName != null)
                        {
                            region = _nameManager.ResolveRegion(regionElementName, _layout.Package);
                            if (region == null)
                                _layout.AddVisualElement(w => w.WriteElement("p", "Unknown region element '" + regionElementName + "'"), null);
                        }

                        if (region != null)
                        {
                            if (_regionComponents.ContainsKey(regionName))
                            {
                                _layout.AddRegion(regionName, region, (IComponent)_regionComponents[regionName]);
                            }
                            else if (_regionLayouts.ContainsKey(regionName))
                            {
                                _layout.AddRegion(regionName, region, (ILayout)_regionLayouts[regionName]);
                            }
                            else
                                _layout.AddRegion(regionName, region);
                        }
                    }
                    else if (childRegionSet != null)
                    {
                        WriteNestingOpeningTag();
                        AddRegionSet(childRegionSet);
                        WriteNestingClosingTag();
                    }
                }
            }
        }

        private void WriteOpeningTag()
        {
            if (!string.IsNullOrEmpty(_tag))
            {
                var attributes = _htmlHelper.StyleAttributes(_style, _classNames, _layout.Package);
                _layout.AddVisualElement(w => w.WriteOpenTag(_tag, attributes), "layout container element");
            }
        }

        private void WriteClosingTag()
        {
            if (!string.IsNullOrEmpty(_tag))
                _layout.AddVisualElement(w => w.WriteCloseTag(_tag), null);
        }

        private void WriteNestingOpeningTag()
        {
            if (!string.IsNullOrEmpty(_nestingTag))
            {
                var attributes = _htmlHelper.StyleAttributes(_nestedStyle, _nestedClassNames, _layout.Package);
                _layout.AddVisualElement(w => w.WriteOpenTag(_nestingTag, attributes), "grouping regions in layout");
            }
        }

        private void WriteNestingClosingTag()
        {
            if (!string.IsNullOrEmpty(_nestingTag))
                _layout.AddVisualElement(w => w.WriteCloseTag(_nestingTag), null);
        }
    }
}
