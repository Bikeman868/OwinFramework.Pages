using System;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Html.Interfaces;

namespace OwinFramework.Pages.Html.Builders
{
    public class RegionDefinition : IRegionDefinition
    {
        private readonly INameManager _nameManager;
        private readonly IHtmlHelper _htmlHelper;
        private readonly BuiltRegion _region;
        private string _tagName = "div";
        private string _style;
        private string[] _classNames;

        public RegionDefinition(
            INameManager nameManager,
            IHtmlHelper htmlHelper,
            IRegionDependenciesFactory regionDependenciesFactory,
            IPackage package)
        {
            _nameManager = nameManager;
            _htmlHelper = htmlHelper;
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
            // TODO: Data binding
            return this;
        }

        IRegionDefinition IRegionDefinition.BindTo<T>() 
        {
            // TODO: Data binding
            return this;
        }

        IRegionDefinition IRegionDefinition.BindTo(Type dataType)
        {
            // TODO: Data binding
            return this;
        }

        IRegionDefinition IRegionDefinition.DataProvider(string dataProviderName)
        {
            // TODO: Data binding
            return this;
        }

        IRegionDefinition IRegionDefinition.DataProvider(IDataProvider dataProvider)
        {
            // TODO: Data binding
            return this;
        }

        IRegionDefinition IRegionDefinition.ForEach(Type dataType, string tag, string style, params string[] classes)
        {
            // TODO: Data binding
            return this;
        }

        IRegionDefinition IRegionDefinition.DeployIn(IModule module)
        {
            _region.Module = module;
            return this;
        }

        IRegionDefinition IRegionDefinition.DataScope(string scopeName)
        {
            return this;
        }

        IRegionDefinition IRegionDefinition.NeedsComponent(string componentName)
        {
            return this;
        }

        IRegionDefinition IRegionDefinition.NeedsComponent(IComponent component)
        {
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

        IRegion IRegionDefinition.Build()
        {
            if (!string.IsNullOrEmpty(_tagName))
            {
                var attributes = _htmlHelper.StyleAttributes(_style, _classNames, _region.Package);
                _region.WriteOpen = w => w.WriteOpenTag(_tagName, attributes);
                _region.WriteClose = w => w.WriteCloseTag(_tagName);
            }

            _nameManager.Register(_region);

            return _region;
        }
    }
}
