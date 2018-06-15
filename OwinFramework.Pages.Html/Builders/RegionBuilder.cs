using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Builders
{
    // TODO: Data binding
    // TODO: Repeating content on binding to a list
    // TODO: Render styles to dynamic assets
    // TODO: Implement AssetDeployment

    internal class RegionBuilder: IRegionBuilder
    {
        private readonly INameManager _nameManager;
        private readonly IHtmlHelper _htmlHelper;

        public RegionBuilder(
            INameManager nameManager,
            IHtmlHelper htmlHelper)
        {
            _nameManager = nameManager;
            _htmlHelper = htmlHelper;
        }

        IRegionDefinition IRegionBuilder.Region()
        {
            return new RegionDefinition(_nameManager, _htmlHelper);
        }

        private class RegionDefinition: IRegionDefinition
        {
            private readonly INameManager _nameManager;
            private readonly IHtmlHelper _htmlHelper;
            private readonly BuiltRegion _region;
            private string _tagName;
            private string _style;
            private string[] _classNames;

            public RegionDefinition(
                INameManager nameManager,
                IHtmlHelper htmlHelper)
            {
                _nameManager = nameManager;
                _htmlHelper = htmlHelper;
                _region = new BuiltRegion();
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
                _region.DefaultContent = layout;
                return this;
            }

            public IRegionDefinition Layout(string layoutName)
            {
                _nameManager.AddResolutionHandler(() => 
                    {
                        _region.DefaultContent = _nameManager.ResolveLayout(layoutName, _region.Package);
                    });
                return this;
            }

            public IRegionDefinition Component(IComponent component)
            {
                _region.DefaultContent = component;
                return this;
            }

            public IRegionDefinition Component(string componentName)
            {
                _nameManager.AddResolutionHandler(() => 
                    {
                        _region.DefaultContent = _nameManager.ResolveComponent(componentName, _region.Package);
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
                return this;
            }

            public IRegionDefinition BindTo<T>() where T : class
            {
                return this;
            }

            public IRegionDefinition BindTo(Type dataType)
            {
                return this;
            }

            public IRegionDefinition DataContext(string dataContextName)
            {
                return this;
            }

            public IRegionDefinition ForEach(Type dataType, string tag = "", string style = "", params string[] classes)
            {
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
                if (!string.IsNullOrEmpty( _tagName))
                {
                    var attributes = _htmlHelper.StyleAttributes(_style, _classNames);
                    _region.WriteOpen = w => w.WriteOpenTag(_tagName, attributes);
                    _region.WriteClose = w => w.WriteCloseTag(_tagName);
                }
                _nameManager.Register(_region);
                return _region;
            }
        }

        private class PopulatedRegion: IElement
        {
            public BuiltRegion Region;
            public IElement Content;

            public ElementType ElementType { get { return ElementType.Region; } }

            string IElement.Name
            {
                get { return Region.Name; }
                set { throw new InvalidOperationException("You can not name a region instance"); }
            }

            IPackage IElement.Package
            {
                get { return Region.Package; }
                set { throw new InvalidOperationException("You can not set the package for a region instance"); }
            }

            IModule IElement.Module
            {
                get { return Region.Module; }
                set { throw new InvalidOperationException("You can not set the module for a region instance"); }
            }

            private AssetDeployment _assetDeployment = AssetDeployment.Inherit;

            public AssetDeployment AssetDeployment
            {
                get { return _assetDeployment == AssetDeployment.Inherit ? Region.AssetDeployment : _assetDeployment; }
                set { _assetDeployment = value; }
            }

            void IElement.Initialize(IInitializationData initializationData)
            {
                var inheritAssetDeployment = AssetDeployment == AssetDeployment.Inherit;
                var assetDeployment = inheritAssetDeployment ? initializationData.AssetDeployment : AssetDeployment;

                initializationData.HasElement(this, assetDeployment, Region.Module);

                if (!inheritAssetDeployment)
                {
                    initializationData.Push();
                    initializationData.AssetDeployment = AssetDeployment;
                }
                try
                {
                    Content.Initialize(initializationData);
                }
                finally
                {
                    if (!inheritAssetDeployment)
                    {
                        initializationData.Pop();
                    }
                }
            }

            IEnumerator<IElement> IElement.GetChildren()
            {
                return null;
            }

            IWriteResult IElement.WriteStaticAssets(AssetType assetType, IHtmlWriter writer)
            {
                return 
                    Region.WriteStaticAssets(assetType, writer)
                    .Add(Content.WriteStaticAssets(assetType, writer));
            }

            IWriteResult IElement.WriteDynamicAssets(AssetType assetType, IHtmlWriter writer)
            {
                return
                    Region.WriteDynamicAssets(assetType, writer)
                    .Add(Content.WriteDynamicAssets(assetType, writer));
            }

            IWriteResult IElement.WriteInitializationScript(IRenderContext renderContext, IDataContext dataContext)
            {
                return
                    Region.WriteInitializationScript(renderContext, dataContext)
                    .Add(Content.WriteInitializationScript(renderContext, dataContext));
            }

            IWriteResult IElement.WriteTitle(IRenderContext renderContext, IDataContext dataContext)
            {
                return
                    Region.WriteTitle(renderContext, dataContext)
                    .Add(Content.WriteTitle(renderContext, dataContext));
            }

            IWriteResult IElement.WriteHead(IRenderContext renderContext, IDataContext dataContext)
            {
                return
                    Region.WriteHead(renderContext, dataContext)
                    .Add(Content.WriteHead(renderContext, dataContext));
            }

            IWriteResult IElement.WriteHtml(IRenderContext renderContext, IDataContext dataContext)
            {
                if (renderContext.IncludeComments)
                    renderContext.Html.WriteComment(
                        (string.IsNullOrEmpty(Region.Name) ? "unnamed" : Region.Name) +
                        (Region.Package == null ? " region element" : " region element from the " + Region.Package.Name + " package"));

                var writeOpen = Region.WriteOpen;
                var writeClose = Region.WriteClose;

                if (writeOpen != null) writeOpen(renderContext.Html);

                // TODO: If this is a repeater then repeat content
                var result = Content == null ? WriteResult.Continue() : Content.WriteHtml(renderContext, dataContext);

                if (writeClose != null) writeClose(renderContext.Html);

                return result;
            }
        }

        private class BuiltRegion: Region
        {
            public Action<IHtmlWriter> WriteOpen;
            public Action<IHtmlWriter> WriteClose;

            private PopulatedRegion _defaultContent;
            public IElement DefaultContent 
            {
                get { return _defaultContent; }
                set { _defaultContent = (PopulatedRegion)Populate(value); } 
            }

            public override IEnumerator<IElement> GetChildren()
            {
                return DefaultContent == null ? null : DefaultContent.GetChildren();
            }

            public override IElement Populate(IElement content)
            {
                if (content == null) return DefaultContent;

                return new PopulatedRegion
                {
                    Region = this,
                    Content = content
                };
            }
        }
    }
}
