using System;
using System.Linq;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Html.Elements;
using OwinFramework.Pages.Html.Interfaces;
using OwinFramework.Pages.Html.Runtime;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Builders
{
    public class RegionDefinition : IRegionDefinition
    {
        private readonly INameManager _nameManager;
        private readonly IHtmlHelper _htmlHelper;
        private readonly IFluentBuilder _fluentBuilder;
        private readonly IComponentDependenciesFactory _componentDependenciesFactory;
        private readonly Region _region;
        private readonly List<FunctionDefinition> _functionDefinitions;
        private readonly List<CssDefinition> _cssDefinitions;

        private string _tagName = "div";
        private string _style;
        private string[] _classNames;

        private string _childTagName;
        private string _childStyle;
        private string[] _childClassNames;

        public RegionDefinition(
            Region region,
            INameManager nameManager,
            IHtmlHelper htmlHelper,
            IFluentBuilder fluentBuilder,
            IComponentDependenciesFactory componentDependenciesFactory,
            IPackage package)
        {
            _region = region;
            _nameManager = nameManager;
            _htmlHelper = htmlHelper;
            _fluentBuilder = fluentBuilder;
            _componentDependenciesFactory = componentDependenciesFactory;

            _cssDefinitions = new List<CssDefinition>();
            _functionDefinitions = new List<FunctionDefinition>();

            if (package != null)
                _region.Package = package;
        }

        IRegionDefinition IRegionDefinition.Name(string name)
        {
            if (string.IsNullOrEmpty(name)) return this;

            _region.Name = name;

            return this;
        }

        IRegionDefinition IRegionDefinition.Tag(string tagName)
        {
            _tagName = tagName;
            return this;
        }

        IRegionDefinition IRegionDefinition.ClassNames(params string[] classNames)
        {
            _classNames = classNames;
            return this;
        }

        IRegionDefinition IRegionDefinition.Style(string style)
        {
            _style = style;
            return this;
        }

        #region Packaging and deployment

        IRegionDefinition IRegionDefinition.PartOf(IPackage package)
        {
            _region.Package = package;
            return this;
        }

        IRegionDefinition IRegionDefinition.PartOf(string packageName)
        {
            if (string.IsNullOrEmpty(packageName)) return this;

            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolvePackageNames,
                (nm, r, n) =>
                {
                    r.Package = nm.ResolvePackage(n);
                },
                _region,
                packageName);

            return this;
        }
        
        IRegionDefinition IRegionDefinition.DeployIn(IModule module)
        {
            _region.Module = module;
            return this;
        }

        IRegionDefinition IRegionDefinition.DeployIn(string moduleName)
        {
            if (string.IsNullOrEmpty(moduleName)) return this;

            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolveElementReferences,
                (nm, r, n) => r.Module = nm.ResolveModule(n),
                _region,
                moduleName);

            return this;
        }

        IRegionDefinition IRegionDefinition.DeployCss(string cssSelector, string cssStyle)
        {
            if (string.IsNullOrEmpty(cssSelector))
                throw new ComponentBuilderException("No CSS selector is specified for component CSS asset");

            if (string.IsNullOrEmpty(cssStyle))
                throw new ComponentBuilderException("No CSS style is specified for component CSS asset");

            var cssDefinition = new CssDefinition
            {
                Selector = cssSelector.Trim(),
                Style = cssStyle.Trim()
            };

            if (!cssDefinition.Style.EndsWith(";"))
                cssDefinition.Style += ";";

            _cssDefinitions.Add(cssDefinition);

            return this;
        }

        IRegionDefinition IRegionDefinition.DeployFunction(string returnType, string functionName, string parameters, string functionBody, bool isPublic)
        {
            var functionDefinition = new FunctionDefinition
            {
                ReturnType = returnType,
                FunctionName = functionName,
                Parameters = parameters,
                Body = functionBody,
                IsPublic = isPublic
            };

            _functionDefinitions.Add(functionDefinition);

            return this;
        }

        #endregion

        #region Defining the contents of the region

        IRegionDefinition IRegionDefinition.AssetDeployment(AssetDeployment assetDeployment)
        {
            _region.AssetDeployment = assetDeployment;
            return this;
        }

        IRegionDefinition IRegionDefinition.Layout(ILayout layout)
        {
            _region.Content = layout;
            return this;
        }

        IRegionDefinition IRegionDefinition.Layout(string layoutName)
        {
            if (string.IsNullOrEmpty(layoutName))
                throw new RegionBuilderException("When defining the region layout a layout name must be specified");

            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolveElementReferences,
                (nm, r, n) => r.Content = nm.ResolveLayout(n, r.Package),
                _region,
                layoutName);

            return this;
        }

        IRegionDefinition IRegionDefinition.Component(IComponent component)
        {
            _region.Content = component;
            return this;
        }

        IRegionDefinition IRegionDefinition.Component(string componentName)
        {
            if (string.IsNullOrEmpty(componentName))
                throw new RegionBuilderException("When defining the region component a component name must be specified");

            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolveElementReferences,
                (nm, r, n) => r.Content = nm.ResolveComponent(n, r.Package),
                _region,
                componentName);

            return this;
        }

        IRegionDefinition IRegionDefinition.Html(string textAssetName, string defaultHtml)
        {
            var htmlComponent = new HtmlComponent(_componentDependenciesFactory);
            htmlComponent.Html(textAssetName, defaultHtml);
            _region.Content = htmlComponent;

            return this;
        }

        IRegionDefinition IRegionDefinition.AddTemplate(string templatePath, PageArea pageArea)
        {
            var templateComponent = (_region.Content as TemplateComponent) ?? new TemplateComponent(_componentDependenciesFactory);

            switch(pageArea)
            {
                case PageArea.Head:
                    templateComponent.HeadTemplate(templatePath);
                    break;
                case PageArea.Scripts:
                    templateComponent.ScriptTemplate(templatePath);
                    break;
                case PageArea.Styles:
                    templateComponent.StyleTemplate(templatePath);
                    break;
                case PageArea.Body:
                    templateComponent.BodyTemplate(templatePath);
                    break;
                case PageArea.Initialization:
                    templateComponent.InitializationTemplate(templatePath);
                    break;
            }

            _region.Content = templateComponent;

            return this;
        }


        public IRegionDefinition ZoneComponent(string zoneName, IComponent component)
        {
            if (_region.LayoutZones == null)
                _region.LayoutZones = new Dictionary<string, IElement>(StringComparer.OrdinalIgnoreCase);

            _region.LayoutZones[zoneName] = component;

            return this;
        }

        IRegionDefinition IRegionDefinition.ZoneComponent(string zoneName, string componentName)
        {
            if (string.IsNullOrEmpty(zoneName))
                throw new PageBuilderException("You must provide a zone name when configuring region layout zones");

            if (string.IsNullOrEmpty(componentName))
                throw new PageBuilderException("You must provide a component name when configuring a region layout zone");

            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolveElementReferences, 
                (nm, n) => ZoneComponent(zoneName, nm.ResolveComponent(n, _region.Package)),
                componentName);

            return this;
        }

        public IRegionDefinition ZoneLayout(string zoneName, ILayout layout)
        {
            if (_region.LayoutZones == null)
                _region.LayoutZones = new Dictionary<string, IElement>(StringComparer.OrdinalIgnoreCase);

            _region.LayoutZones[zoneName] = layout;

            return this;
        }

        IRegionDefinition IRegionDefinition.ZoneLayout(string zoneName, string layoutName)
        {
            if (string.IsNullOrEmpty(zoneName))
                throw new PageBuilderException("You must provide a zone name when configuring region layout zones");

            if (string.IsNullOrEmpty(layoutName))
                throw new PageBuilderException("You must provide a layout name when configuring a region layout zone");

            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolveElementReferences, 
                (nm, n) => ZoneLayout(zoneName, nm.ResolveLayout(n, _region.Package)),
                layoutName);

            return this;
        }

        public IRegionDefinition ZoneRegion(string zoneName, IRegion region)
        {
            if (_region.LayoutZones == null)
                _region.LayoutZones = new Dictionary<string, IElement>(StringComparer.OrdinalIgnoreCase);

            _region.LayoutZones[zoneName] = region;

            return this;
        }

        IRegionDefinition IRegionDefinition.ZoneRegion(string zoneName, string regionName)
        {
            if (string.IsNullOrEmpty(zoneName))
                throw new PageBuilderException("You must provide a zone name when configuring region layout zones");

            if (string.IsNullOrEmpty(regionName))
                throw new PageBuilderException("You must provide a region name when configuring a region layout zone");

            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolveElementReferences, 
                (nm, n) => ZoneRegion(zoneName, nm.ResolveRegion(n, _region.Package)),
                regionName);

            return this;
        }

        IRegionDefinition IRegionDefinition.ZoneHtml(string zoneName, string textAssetName, string defaultHtml)
        {
            if (string.IsNullOrEmpty(zoneName))
                throw new PageBuilderException("You must provide a zone name when configuring region layout zones");

            var component = new HtmlComponent(_componentDependenciesFactory);
            component.Html(textAssetName, defaultHtml);

            return ZoneComponent(zoneName, component);
        }

        IRegionDefinition IRegionDefinition.ZoneTemplate(string zoneName, string templatePath)
        {
            if (string.IsNullOrEmpty(zoneName))
                throw new PageBuilderException("You must provide a zone name when configuring page regions");

            var component = new TemplateComponent(_componentDependenciesFactory);
            component.BodyTemplate(templatePath);

            return ZoneComponent(zoneName, component);
        }

        #endregion

        #region Data binding and repetition

        IRegionDefinition IRegionDefinition.ForEach<T>(
            string scopeName, 
            string childTag, 
            string childStyle, 
            string listScope, 
            params string[] childClassNames)
        {
            return ((IRegionDefinition)this).ForEach(
                typeof(T),
                scopeName,
                childTag,
                childStyle,
                listScope,
                childClassNames);
        }

        IRegionDefinition IRegionDefinition.ForEach(
            Type dataType, 
            string scopeName, 
            string childTag, 
            string childStyle, 
            string listScope, 
            params string[] childClassNames)
        {
            if (dataType == null)
                throw new RegionBuilderException("When configuring a region to repeat the data type to repeat must be specified");

            _region.RepeatType = dataType;
            _region.RepeatScope = scopeName;
            _region.ListScope = listScope;

            _childTagName = childTag;
            _childStyle = childStyle;
            _childClassNames = childClassNames;

            return this;
        }

        public IRegionDefinition DataScope<T>(string scopeName)
        {
            return DataScope(typeof(T), scopeName);
        }

        public IRegionDefinition DataScope(Type dataType, string scopeName)
        {
            var dataScope = _region as IDataScopeRules;
            if (ReferenceEquals(dataScope, null))
               throw new RegionBuilderException("This region is not a data scope provider");

            dataScope.AddScope(dataType, scopeName);

            return this;
        }

        IRegionDefinition IRegionDefinition.BindTo<T>(string scopeName) 
        {
            var dataConsumer = _region as IDataConsumer;
            if (dataConsumer == null)
                throw new RegionBuilderException("This region is not a consumer of data");

            dataConsumer.HasDependency<T>(scopeName);

            return this;
        }

        IRegionDefinition IRegionDefinition.BindTo(Type dataType, string scopeName)
        {
            if (dataType == null)
                throw new RegionBuilderException("To define data binding you must specify the type of data to bind");

            var dataConsumer = _region as IDataConsumer;
            if (dataConsumer == null)
                throw new RegionBuilderException("This region is not a consumer of data");

            dataConsumer.HasDependency(dataType, scopeName);

            return this;
        }

        IRegionDefinition IRegionDefinition.DataProvider(string dataProviderName)
        {
            var dataConsumer = _region as IDataConsumer;
            if (dataConsumer == null)
                throw new RegionBuilderException("This region is not a consumer of data");

            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolveElementReferences,
                (nm, r, n) => ((IDataConsumer)r).HasDependency(nm.ResolveDataProvider(n, r.Package)),
                _region,
                dataProviderName);

            return this;
        }

        IRegionDefinition IRegionDefinition.DataProvider(IDataProvider dataProvider)
        {
            var dataConsumer = _region as IDataConsumer;
            if (dataConsumer == null)
                throw new RegionBuilderException("This region is not a consumer of data");

            dataConsumer.HasDependency(dataProvider);

            return this;
        }

        #endregion

        #region Dependencies

        IRegionDefinition IRegionDefinition.NeedsComponent(string componentName)
        {
            if (string.IsNullOrEmpty(componentName))
                throw new RegionBuilderException("No component name provided in region dependency");

            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolveElementReferences,
                (nm, r, n) => r.NeedsComponent(nm.ResolveComponent(n, r.Package)),
                _region,
                componentName);

            return this;
        }

        IRegionDefinition IRegionDefinition.NeedsComponent(IComponent component)
        {
            if (ReferenceEquals(component, null))
                throw new RegionBuilderException("Null component reference for dependent component");

            _region.NeedsComponent(component);

            return this;
        }

        #endregion

        IRegion IRegionDefinition.Build()
        {
            if (_cssDefinitions.Count > 0)
            {
                _nameManager.AddResolutionHandler(
                    NameResolutionPhase.ResolveAssetNames,
                    () =>
                    {
                        _region.CssRules = _cssDefinitions
                            .Select(d =>
                            {
                                d.Selector = _htmlHelper.NamespaceCssSelector(d.Selector, _region.Package);
                                return d;
                            })
                            .Select(d =>
                            {
                                Action<ICssWriter> writeAction = w => w.WriteRule(d.Selector, d.Style);
                                return writeAction;
                            })
                            .ToArray();
                    });
            }

            if (_functionDefinitions.Count > 0)
            {
                _nameManager.AddResolutionHandler(
                    NameResolutionPhase.ResolveAssetNames,
                    () =>
                    {
                        _region.JavascriptFunctions = _functionDefinitions
                            .Select(d =>
                            {
                                Action<IJavascriptWriter> writeAction = w => w.WriteFunction(
                                    d.FunctionName, d.Parameters, d.Body, d.ReturnType, _region.Package, d.IsPublic);
                                return writeAction;
                            })
                            .ToArray();
                    });
            }

            if (!string.IsNullOrEmpty(_tagName))
            {
                _nameManager.AddResolutionHandler(
                    NameResolutionPhase.ResolveElementReferences,
                    nm =>
                    {
                        var attributes = _htmlHelper.StyleAttributes(_style, _classNames, _region.Package);
                        _region.WriteOpen = w => { w.WriteOpenTag(_tagName, attributes); w.WriteLine(); };
                        _region.WriteClose = w => { w.WriteCloseTag(_tagName); w.WriteLine(); };
                    });
            }

            if (!string.IsNullOrEmpty(_childTagName))
            {
                _nameManager.AddResolutionHandler(
                    NameResolutionPhase.ResolveElementReferences,
                    nm =>
                    {
                        var attributes = _htmlHelper.StyleAttributes(_childStyle, _childClassNames, _region.Package);
                        _region.WriteChildOpen = w => { w.WriteOpenTag(_childTagName, attributes); w.WriteLine(); };
                        _region.WriteChildClose = w => { w.WriteCloseTag(_childTagName); w.WriteLine(); };
                    });
            }

            _fluentBuilder.Register(_region);
            return _region;
        }
    }
}
