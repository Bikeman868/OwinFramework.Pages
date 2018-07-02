using System;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Html.Interfaces;

namespace OwinFramework.Pages.Html.Builders
{
    public class RegionDefinition : IRegionDefinition
    {
        private readonly INameManager _nameManager;
        private readonly IHtmlHelper _htmlHelper;
        private readonly IFluentBuilder _fluentBuilder;
        private readonly BuiltRegion _region;
        private string _tagName = "div";
        private string _style;
        private string[] _classNames;

        private string _childTagName;
        private string _childStyle;
        private string[] _childClassNames;

        public RegionDefinition(
            INameManager nameManager,
            IHtmlHelper htmlHelper,
            IFluentBuilder fluentBuilder,
            IRegionDependenciesFactory regionDependenciesFactory,
            IPackage package)
        {
            _nameManager = nameManager;
            _htmlHelper = htmlHelper;
            _fluentBuilder = fluentBuilder;
            _region = new BuiltRegion(regionDependenciesFactory);
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
            _nameManager.AddResolutionHandler(() =>
            {
                _region.Populate(_nameManager.ResolveLayout(layoutName, _region.Package));
            });
            return this;
        }

        IRegionDefinition IRegionDefinition.Component(IComponent component)
        {
            _region.Populate(component);
            return this;
        }

        IRegionDefinition IRegionDefinition.Component(string componentName)
        {
            _nameManager.AddResolutionHandler(() =>
            {
                _region.Populate(_nameManager.ResolveComponent(componentName, _region.Package));
            });
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

        IRegionDefinition IRegionDefinition.ForEach<T>(string tag, string style, params string[] classes)
        {
            _region.RepeatType = typeof(T);
            _childTagName = tag;
            _childStyle = style;
            _childClassNames = classes;
            return this;
        }

        IRegionDefinition IRegionDefinition.ForEach(Type dataType, string tag, string style, params string[] classes)
        {
            _region.RepeatType = dataType;
            _childTagName = tag;
            _childStyle = style;
            _childClassNames = classes;
            return this;
        }

        public IRegionDefinition DataScope(Type type, string scopeName)
        {
            var dataScope = _region as IDataScopeProvider;
            if (dataScope != null)
                dataScope.AddScope(null, scopeName);
            return this;
        }

        IRegionDefinition IRegionDefinition.BindTo<T>(string scopeName) 
        {
            var dataConsumer = _region as IDataConsumer;
            if (dataConsumer != null)
                dataConsumer.NeedsData<T>(scopeName);
            return this;
        }

        IRegionDefinition IRegionDefinition.BindTo(Type dataType, string scopeName)
        {
            var dataConsumer = _region as IDataConsumer;
            if (dataConsumer != null)
                dataConsumer.NeedsData(dataType, scopeName);
            return this;
        }

        IRegionDefinition IRegionDefinition.DataProvider(string dataProviderName)
        {
            var dataConsumer = _region as IDataConsumer;
            if (dataConsumer != null)
            {
                _nameManager.AddResolutionHandler(
                    (nm, c) => c.NeedsProvider(nm.ResolveDataProvider(dataProviderName, _region.Package)),
                    dataConsumer);
            }
            return this;
        }

        IRegionDefinition IRegionDefinition.DataProvider(IDataProvider dataProvider)
        {
            var dataConsumer = _region as IDataConsumer;
            if (dataConsumer != null)
                dataConsumer.NeedsProvider(dataProvider);
            return this;
        }

        IRegionDefinition IRegionDefinition.DeployIn(IModule module)
        {
            _region.Module = module;
            return this;
        }

        IRegionDefinition IRegionDefinition.NeedsComponent(string componentName)
        {
            _nameManager.AddResolutionHandler(() =>
            {
                _region.NeedsComponent(_nameManager.ResolveComponent(componentName, _region.Package));
            });
            return this;
        }

        IRegionDefinition IRegionDefinition.NeedsComponent(IComponent component)
        {
            _region.NeedsComponent(component);
            return this;
        }

        IRegionDefinition IRegionDefinition.DeployIn(string moduleName)
        {
            _nameManager.AddResolutionHandler(() =>
            {
                _region.Module = _nameManager.ResolveModule(moduleName);
            });
            return this;
        }

        IRegion IRegionDefinition.Build(Type type)
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

            return _fluentBuilder.Register(_region, type);
        }
    }
}
