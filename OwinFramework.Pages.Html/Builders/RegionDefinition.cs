using System;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;

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
            IRegionDependenciesFactory regionDependenciesFactory)
        {
            _nameManager = nameManager;
            _htmlHelper = htmlHelper;
            _region = new BuiltRegion(regionDependenciesFactory);
        }

        public IRegionDefinition Name(string name)
        {
            _region.Name = name;
            return this;
        }

        public IRegionDefinition PartOf(IPackage package)
        {
            _region.Package = package;
            return this;
        }

        public IRegionDefinition PartOf(string packageName)
        {
            _region.Package = _nameManager.ResolvePackage(packageName);

            if (_region.Package == null)
                throw new RegionBuilderException(
                    "Package names must be registered before regions can refer to them. " +
                    "There is no registered package '" + packageName + "'");
            return this;
        }

        public IRegionDefinition AssetDeployment(AssetDeployment assetDeployment)
        {
            _region.AssetDeployment = assetDeployment;
            return this;
        }

        public IRegionDefinition Layout(ILayout layout)
        {
            _region.Populate(layout);
            return this;
        }

        public IRegionDefinition Layout(string layoutName)
        {
            _nameManager.AddResolutionHandler(() =>
            {
                _region.Populate(_nameManager.ResolveLayout(layoutName, _region.Package));
            });
            return this;
        }

        public IRegionDefinition Component(IComponent component)
        {
            _region.Populate(component);
            return this;
        }

        public IRegionDefinition Component(string componentName)
        {
            _nameManager.AddResolutionHandler(() =>
            {
                _region.Populate(_nameManager.ResolveComponent(componentName, _region.Package));
            });
            return this;
        }

        public IRegionDefinition Tag(string tagName)
        {
            _tagName = tagName;
            return this;
        }

        public IRegionDefinition ClassNames(params string[] classNames)
        {
            _classNames = classNames;
            return this;
        }

        public IRegionDefinition Style(string style)
        {
            _style = style;
            return this;
        }

        public IRegionDefinition ForEach<T>()
        {
            // TODO: Data binding
            return this;
        }

        public IRegionDefinition BindTo<T>() where T : class
        {
            // TODO: Data binding
            return this;
        }

        public IRegionDefinition BindTo(Type dataType)
        {
            // TODO: Data binding
            return this;
        }

        public IRegionDefinition DataContext(string dataContextName)
        {
            // TODO: Data binding
            return this;
        }

        public IRegionDefinition ForEach(Type dataType, string tag = "", string style = "", params string[] classes)
        {
            // TODO: Data binding
            return this;
        }

        IRegionDefinition IRegionDefinition.DeployIn(IModule module)
        {
            _region.Module = module;
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

        public IRegion Build()
        {
            if (!string.IsNullOrEmpty(_tagName))
            {
                var attributes = _htmlHelper.StyleAttributes(_style, _classNames);
                _region.WriteOpen = w => w.WriteOpenTag(_tagName, attributes);
                _region.WriteClose = w => w.WriteCloseTag(_tagName);
            }

            _nameManager.Register(_region);

            return _region;
        }
    }
}
