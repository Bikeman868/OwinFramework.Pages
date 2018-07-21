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
            if (string.IsNullOrEmpty(name)) return this;

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
            if (string.IsNullOrEmpty(packageName)) return this;

            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolvePackageNames,
                (nm, r, n) => r.Package = nm.ResolvePackage(n),
                _region,
                packageName);

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
            if (string.IsNullOrEmpty(layoutName))
                throw new RegionBuilderException("When defining the region layout a layout name must be specified");

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
            if (string.IsNullOrEmpty(componentName))
                throw new RegionBuilderException("When defining the region component a component name must be specified");

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

        IRegionDefinition IRegionDefinition.ForEach<T>(
            string scopeName, 
            string tag, 
            string style, 
            string listScope, 
            params string[] classes)
        {
            return ((IRegionDefinition)this).ForEach(
                typeof(T),
                scopeName,
                tag,
                style,
                listScope,
                classes);
        }

        IRegionDefinition IRegionDefinition.ForEach(
            Type dataType, 
            string scopeName, 
            string tag, 
            string style, 
            string listScope, 
            params string[] classes)
        {
            if (dataType == null)
                throw new RegionBuilderException("When configuring a region to repeat the data type to repeat must be specified");

            _region.RepeatType = dataType;
            _region.RepeatScope = scopeName;
            _region.ListScope = listScope;

            _childTagName = tag;
            _childStyle = style;
            _childClassNames = classes;

            var dataScope = _region as IDataScopeProvider;
            dataScope.AddScope(_region.RepeatType, _region.RepeatScope);

            var dataConsumer = _region as IDataConsumer;
            dataConsumer.HasDependency(_region.ListType, _region.ListScope);

            return this;
        }

        public IRegionDefinition DataScope(Type dataType, string scopeName)
        {
            var dataScope = _region as IDataScopeProvider;
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
                (nm, c, n) => c.HasDependency(nm.ResolveDataProvider(n)),
                dataConsumer,
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

        IRegionDefinition IRegionDefinition.DeployIn(IModule module)
        {
            _region.Module = module;
            return this;
        }

        IRegionDefinition IRegionDefinition.NeedsComponent(string componentName)
        {
            if (string.IsNullOrEmpty(componentName))
                throw new RegionBuilderException("No component name provided in region dependency");

            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolveElementReferences,
                (nm, r, n) => r.NeedsComponent(nm.ResolveComponent(n)),
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
