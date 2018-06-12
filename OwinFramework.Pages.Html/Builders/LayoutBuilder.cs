using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
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

            private RegionSet _regionSet;
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

                        Action<RegionSet> addRegionSet = null;

                        addRegionSet = rs =>
                            {
                                if (rs == null || rs.Elements == null) return;
                                foreach (var e in rs.Elements)
                                {
                                    if (e is string)
                                    {
                                        var regionName = (string)e;
                                        if (!_regions.ContainsKey(regionName))
                                        {
                                            _layout.Elements.Add(new StaticHtmlElement { WriteAction = w => w.WriteElement("p",  "Layout does not have a '" + e + "' region") });
                                            continue;
                                        }

                                        var regionElement = _regions[regionName];
                                        var region = regionElement as IRegion;
                                        if (region != null)
                                        {
                                            _layout.Elements.Add(region);
                                        }
                                        else
                                        {
                                            var regionElementName = (string)regionElement;
                                            region = _nameManager.ResolveRegion(regionElementName, _layout.Package);
                                            if (region == null)
                                                _layout.Elements.Add(new StaticHtmlElement { WriteAction = w => w.WriteElement("p",  "Unknown region element '" + regionElementName + "'</b></p>") });
                                            else
                                                _layout.Elements.Add(region);
                                        }
                                    }
                                    else if (e is RegionSet)
                                    {
                                        _layout.Elements.Add(new StaticHtmlElement { WriteAction = w => w.WriteOpenTag("div", "class", "region-set") });
                                        addRegionSet((RegionSet)e);
                                        _layout.Elements.Add(new StaticHtmlElement { WriteAction = w => w.WriteCloseTag("div") });
                                    }
                                }
                            };

                        addRegionSet(_regionSet);
                    });
                return _layout;
            }
        }

        private class StaticHtmlElement: Element
        {
            public Action<IHtmlWriter> WriteAction;

            public override IWriteResult WriteHtml(IRenderContext renderContext, IDataContext dataContext)
            {
                WriteAction(renderContext.Html);
                return WriteResult.Continue();
            }
        }

        private class BuiltLayout : Layout
        {
            public List<IElement> Elements;
            public AssetDeployment AssetDeployment;

            public override IWriteResult WriteHtml(IRenderContext renderContext, IDataContext dataContext)
            {
                foreach (var element in Elements)
                    element.WriteHtml(renderContext, dataContext);

                return WriteResult.Continue();
            }
        }
    }
}
