using System;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Html.Interfaces;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Builders
{
    public class RegionDefinition : IRegionDefinition
    {
        private readonly INameManager _nameManager;
        private readonly IHtmlHelper _htmlHelper;
        private readonly IFluentBuilder _fluentBuilder;
        private readonly Region _region;
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
            IPackage package)
        {
            _region = region;
            _nameManager = nameManager;
            _htmlHelper = htmlHelper;
            _fluentBuilder = fluentBuilder;

            if (package != null)
                _region.Package = package;
        }

        IRegionDefinition IRegionDefinition.Name(string name)
        {
            _region.Name = name;
            return this;
        }

        IRegionDefinition IRegionDefinition.PartOf(IPackage package)
        {
            _region.Package = package;
            return this;
        }

        IRegionDefinition IRegionDefinition.PartOf(string packageName)
        {
            _region.Package = _nameManager.ResolvePackage(packageName);

            if (_region.Package == null)
                throw new RegionBuilderException(
                    "Package names must be registered before regions can refer to them. " +
                    "There is no registered package '" + packageName + "'");
            return this;
        }

        IRegionDefinition IRegionDefinition.AssetDeployment(AssetDeployment assetDeployment)
        {
            _region.AssetDeployment = assetDeployment;
            return this;
        }

        IRegionDefinition IRegionDefinition.Layout(ILayout layout)
        {
            _region.Populate(layout);
            return this;
        }

        IRegionDefinition IRegionDefinition.Layout(string layoutName)
        {
            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolveElementReferences,
                (nm, r, n) => r.Populate(nm.ResolveLayout(n, r.Package)),
                _region,
                layoutName);
            return this;
        }

        IRegionDefinition IRegionDefinition.Component(IComponent component)
        {
            _region.Populate(component);
            return this;
        }

        IRegionDefinition IRegionDefinition.Component(string componentName)
        {
            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolveElementReferences,
                (nm, r, n) => r.Populate(nm.ResolveComponent(n, r.Package)),
                _region,
                componentName);
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

        IRegionDefinition IRegionDefinition.ForEach<T>(string scopeName, string tag, string style, string listScope, params string[] classes)
        {
            _region.RepeatType = typeof(T);
            _region.RepeatScope = scopeName;
            _region.ListScope = listScope;

            _childTagName = tag;
            _childStyle = style;
            _childClassNames = classes;
            return this;
        }

        IRegionDefinition IRegionDefinition.ForEach(Type dataType, string scopeName, string tag, string style, string listScope, params string[] classes)
        {
            _region.RepeatType = dataType;
            _region.RepeatScope = scopeName;
            _region.ListScope = listScope;

            _childTagName = tag;
            _childStyle = style;
            _childClassNames = classes;

            return this;
        }

        public IRegionDefinition DataScope(Type type, string scopeName)
        {
            // TODO: This applies to the region instance not the region
            var dataScope = _region as IDataScopeProvider;
            if (dataScope != null)
                dataScope.AddScope(type, scopeName);
            return this;
        }

        IRegionDefinition IRegionDefinition.BindTo<T>(string scopeName) 
        {
            var dataConsumer = _region as IDataConsumer;
            if (dataConsumer != null)
                dataConsumer.HasDependency<T>(scopeName);
            return this;
        }

        IRegionDefinition IRegionDefinition.BindTo(Type dataType, string scopeName)
        {
            var dataConsumer = _region as IDataConsumer;
            if (dataConsumer != null)
                dataConsumer.HasDependency(dataType, scopeName);
            return this;
        }

        IRegionDefinition IRegionDefinition.DataProvider(string dataProviderName)
        {
            var dataConsumer = _region as IDataConsumer;
            if (dataConsumer != null)
            {
                _nameManager.AddResolutionHandler(
                    NameResolutionPhase.ResolveElementReferences,
                    (nm, c, n) => c.HasDependency(nm.ResolveDataProvider(n, _region.Package)),
                    dataConsumer,
                    dataProviderName);
            }
            return this;
        }

        IRegionDefinition IRegionDefinition.DataProvider(IDataProvider dataProvider)
        {
            var dataConsumer = _region as IDataConsumer;
            if (dataConsumer != null)
                dataConsumer.HasDependency(dataProvider);
            return this;
        }

        IRegionDefinition IRegionDefinition.DeployIn(IModule module)
        {
            _region.Module = module;
            return this;
        }

        IRegionDefinition IRegionDefinition.NeedsComponent(string componentName)
        {
            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolveElementReferences,
                (nm, r, n) => r.NeedsComponent(nm.ResolveComponent(n, r.Package)),
                _region,
                componentName);
            return this;
        }

        IRegionDefinition IRegionDefinition.NeedsComponent(IComponent component)
        {
            _region.NeedsComponent(component);
            return this;
        }

        IRegionDefinition IRegionDefinition.DeployIn(string moduleName)
        {
            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolveElementReferences,
                (nm, r, n) => r.Module = nm.ResolveModule(n),
                _region,
                moduleName);
            return this;
        }

        IRegion IRegionDefinition.Build()
        {
            if (!string.IsNullOrEmpty(_tagName))
            {
                var attributes = _htmlHelper.StyleAttributes(_style, _classNames, _region.Package);
                _region.WriteOpen = w => w.WriteOpenTag(_tagName, attributes);
                _region.WriteClose = w => w.WriteCloseTag(_tagName);
            }

            if (!string.IsNullOrEmpty(_childTagName))
            {
                var attributes = _htmlHelper.StyleAttributes(_childStyle, _childClassNames, _region.Package);
                _region.WriteChildOpen = w => w.WriteOpenTag(_childTagName, attributes);
                _region.WriteChildClose = w => w.WriteCloseTag(_childTagName);
            }

            _fluentBuilder.Register(_region);
            return _region;
        }
    }
}
