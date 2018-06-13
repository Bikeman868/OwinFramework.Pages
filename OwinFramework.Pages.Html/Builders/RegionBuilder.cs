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
                _region.Contents = layout;
                return this;
            }

            public IRegionDefinition Layout(string layoutName)
            {
                _nameManager.AddResolutionHandler(() => 
                    {
                        _region.Contents = _nameManager.ResolveLayout(layoutName, _region.Package);
                    });
                return this;
            }

            public IRegionDefinition Component(IComponent component)
            {
                _region.Contents = component;
                return this;
            }

            public IRegionDefinition Component(string componentName)
            {
                _nameManager.AddResolutionHandler(
                    () => 
                        {
                            _region.Contents = _nameManager.ResolveComponent(componentName, _region.Package);
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

        private class BuiltRegion: Region
        {
            public Action<IHtmlWriter> WriteOpen;
            public Action<IHtmlWriter> WriteClose;
            public IElement Contents;

            private BuiltRegion _parent;

            public override IEnumerator<IElement> GetChildren()
            {
                return Contents == null ? null : Contents.AsEnumerable().GetEnumerator();
            }

            public override IRegion Wrap(IElement content)
            {
                return new BuiltRegion
                    {
                        _parent = this,
                        Contents = content,
                        Name = Name,
                        Module = Module,
                        Package = Package
                    };
            }

            public override IWriteResult WriteDynamicAssets(AssetType assetType, IHtmlWriter writer)
            {
                if (Contents == null) return WriteResult.Continue();
                return Contents.WriteDynamicAssets(assetType, writer);
            }

            public override IWriteResult WriteHead(IRenderContext renderContext, IDataContext dataContext)
            {
                if (Contents == null) return WriteResult.Continue();
                return Contents.WriteHead(renderContext, dataContext);
            }

            public override IWriteResult WriteHtml(IRenderContext renderContext, IDataContext dataContext)
            {
                if (renderContext.IncludeComments)
                    renderContext.Html.WriteComment(
                        (string.IsNullOrEmpty(Name) ? "Unnamed" : Name) +
                        (Package == null ? " region element" : " region element from the " + Package.Name + " package"));

                var writeOpen = _parent == null ? WriteOpen : _parent.WriteOpen;
                var writeClose = _parent == null ? WriteClose : _parent.WriteClose;

                if (writeOpen != null) writeOpen(renderContext.Html);
                var result = Contents == null ? WriteResult.Continue() : Contents.WriteHtml(renderContext, dataContext);
                if (writeClose != null) writeClose(renderContext.Html);

                return result;
            }

            public override IWriteResult WriteInitializationScript(IRenderContext renderContext, IDataContext dataContext)
            {
                if (Contents == null) return WriteResult.Continue();
                return Contents.WriteInitializationScript(renderContext, dataContext);
            }

            public override IWriteResult WriteStaticAssets(AssetType assetType, IHtmlWriter writer)
            {
                if (Contents == null) return WriteResult.Continue();
                return Contents.WriteStaticAssets(assetType, writer);
            }

            public override IWriteResult WriteTitle(IRenderContext renderContext, IDataContext dataContext)
            {
                if (Contents == null) return WriteResult.Continue();
                return Contents.WriteTitle(renderContext, dataContext);
            }
        }
    }
}
