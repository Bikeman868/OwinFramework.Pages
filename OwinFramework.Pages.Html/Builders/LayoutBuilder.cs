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
    // TODO: Data binding
    // TODO: Render styles to dynamic assets
    // TODO: Implement AssetDeployment
    // TODO: Pages can override the contents of regions in the layout

    internal class LayoutBuilder : ILayoutBuilder
    {
        private readonly INameManager _nameManager;
        private readonly IHtmlHelper _htmlHelper;

        public LayoutBuilder(
            INameManager nameManager,
            IHtmlHelper htmlHelper)
        {
            _nameManager = nameManager;
            _htmlHelper = htmlHelper;
        }

        ILayoutDefinition ILayoutBuilder.Layout()
        {
            return new LayoutDefinition(_nameManager, _htmlHelper);
        }

        private class LayoutDefinition: ILayoutDefinition
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
                IHtmlHelper htmlHelper)
            {
                _nameManager = nameManager;
                _htmlHelper = htmlHelper;

                _layout = new BuiltLayout();
                _regionElements = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
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
                        var regionComponentKeys = _regionComponents.Keys.ToList();
                        foreach (var regionName in regionComponentKeys)
                        {
                            var componentRef = _regionComponents[regionName];
                            var componentName = componentRef as string;
                            if (componentName != null)
                                _regionComponents[regionName] = _nameManager.ResolveComponent(componentName);
                        }

                        var regionLayoutKeys = _regionLayouts.Keys.ToList();
                        foreach (var regionName in regionLayoutKeys)
                        {
                            var layoutRef = _regionLayouts[regionName];
                            var layoutName = layoutRef as string;
                            if (layoutName != null)
                                _regionLayouts[regionName] = _nameManager.ResolveLayout(layoutName);
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
                            var regionElementName = (string)regionElement;

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
                    var attributes = _htmlHelper.StyleAttributes(_style, _classNames);
                    _layout.AddVisualElement(w => w.WriteOpenTag(_tag, attributes), "Layout container element");
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
                    var attributes = _htmlHelper.StyleAttributes(_nestedStyle, _nestedClassNames);
                    _layout.AddVisualElement(w => w.WriteOpenTag(_nestingTag, attributes), "Element grouping regions in layout");
                }
            }

            private void WriteNestingClosingTag()
            {
                if (!string.IsNullOrEmpty(_nestingTag))
                    _layout.AddVisualElement(w => w.WriteCloseTag(_nestingTag), null);
            }
        }

        private class StaticHtmlElement: Element
        {
            public Action<IHtmlWriter> WriteAction;
            public string Comment;

            public override IWriteResult WriteHtml(IRenderContext renderContext, IDataContext dataContext)
            {
                if (renderContext.IncludeComments && !string.IsNullOrEmpty(Comment))
                    renderContext.Html.WriteComment(Comment);

                WriteAction(renderContext.Html);
                return WriteResult.Continue();
            }
        }

        private class BuiltLayout : Layout
        {
            private List<IElement> _visualElements;
            private Dictionary<int, int> _visualElementMapping;
            private List<string> _regionNames;
            private List<IRegion> _regions;

            public void AddVisualElement(Action<IHtmlWriter> writeAction, string comment)
            {
                if (_visualElements == null)
                    _visualElements = new List<IElement>();
                _visualElements.Add(new StaticHtmlElement {  WriteAction = writeAction, Comment = comment });
            }

            public void AddRegion(string regionName, IRegion region, IElement element = null)
            {
                if (_regions == null)
                    _regions = new List<IRegion>();
                _regions.Add(region);

                if (_regionNames == null)
                    _regionNames = new List<string>();
                _regionNames.Add(regionName);

                if (_visualElements == null)
                    _visualElements = new List<IElement>();
                _visualElements.Add(element == null ? region : region.Wrap(element));

                if (_visualElementMapping == null)
                    _visualElementMapping = new Dictionary<int, int>();
                _visualElementMapping[_regions.Count - 1] = _visualElements.Count - 1;
            }

            public override void PopulateRegion(string regionName, IElement element)
            {
                for (var i = 0; i < _regionNames.Count; i++)
                {
                    if (string.Equals(_regionNames[i], regionName, StringComparison.OrdinalIgnoreCase))
                    {
                        _visualElements[_visualElementMapping[i]] = _regions[i].Wrap(element);
                    }
                }
            }

            public override IWriteResult WriteStaticAssets(AssetType assetType, IHtmlWriter writer)
            {
                var result = WriteResult.Continue();

                foreach (var region in _regions)
                {
                    var regionResult = region.WriteStaticAssets(assetType, writer);
                    result.Add(regionResult);

                    if (regionResult.IsComplete) break;
                }

                return result;
            }

            public override IWriteResult WriteHead(IRenderContext renderContext, IDataContext dataContext)
            {
                var result = WriteResult.Continue();

                foreach (var region in _regions)
                {
                    var regionResult = region.WriteHead(renderContext, dataContext);
                    result.Add(regionResult);

                    if (regionResult.IsComplete) break;
                }

                return result;
            }

            public override IWriteResult WriteDynamicAssets(AssetType assetType, IHtmlWriter writer)
            {
                var result = WriteResult.Continue();

                foreach (var region in _regions)
                {
                    var regionResult = region.WriteDynamicAssets(assetType, writer);
                    result.Add(regionResult);

                    if (regionResult.IsComplete) break;
                }

                return result;
            }

            public override IWriteResult WriteInitializationScript(IRenderContext renderContext, IDataContext dataContext)
            {
                var result = WriteResult.Continue();

                foreach (var region in _regions)
                {
                    var regionResult = region.WriteInitializationScript(renderContext, dataContext);
                    result.Add(regionResult);

                    if (regionResult.IsComplete) break;
                }

                return result;
            }

            public override IWriteResult WriteTitle(IRenderContext renderContext, IDataContext dataContext)
            {
                var result = WriteResult.Continue();

                foreach (var region in _regions)
                {
                    var regionResult = region.WriteTitle(renderContext, dataContext);
                    result.Add(regionResult);

                    if (regionResult.IsComplete) break;
                }

                return result;
            }

            public override IWriteResult WriteHtml(IRenderContext renderContext, IDataContext dataContext)
            {
                var result = WriteResult.Continue();

                if (renderContext.IncludeComments)
                    renderContext.Html.WriteComment(
                        (string.IsNullOrEmpty(Name) ? "(unnamed)" : Name) +
                        (Package == null ? " layout" : " layout from " + Package.Name + " package"));

                foreach (var element in _visualElements)
                    result.Add(element.WriteHtml(renderContext, dataContext));

                return result;
            }
        }
    }
}
