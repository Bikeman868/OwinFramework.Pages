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
    // TODO: Repeating content on binding to a list
    // TODO: Render styles to dynamic assets
    // TODO: Implement AssetDeployment

    internal class RegionBuilder: IRegionBuilder
    {
        private readonly INameManager _nameManager;

        public RegionBuilder(
                INameManager nameManager)
        {
            _nameManager = nameManager;
        }

        IRegionDefinition IRegionBuilder.Region()
        {
            return new RegionDefinition(_nameManager);
        }

        private class RegionDefinition: IRegionDefinition
        {
            private readonly INameManager _nameManager;
            private readonly BuiltRegion _region;
            private string _tagName;
            private string _style;
            private string[] _classNames;

            public RegionDefinition(
                INameManager nameManager)
            {
                _nameManager = nameManager;
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

            public IRegionDefinition AssetDeployment(Core.Enums.AssetDeployment assetDeployment)
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
                _nameManager.AddResolutionHandler(
                    () => _region.Contents = _nameManager.ResolveLayout(layoutName, _region.Package));
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

            public IRegion Build()
            {
                if (!string.IsNullOrEmpty( _tagName))
                {
                    var tagAttributes = new List<string>();

                    if (!string.IsNullOrEmpty(_style))
                    {
                        tagAttributes.Add("style");
                        tagAttributes.Add(_style);
                    }

                    if (_classNames != null && _classNames.Length > 0)
                    {
                        var classes = string.Join(" ", _classNames
                            .Select(c => c.Trim().Replace(' ', '-'))
                            .Where(c => !string.IsNullOrEmpty(c)));
                        if (classes.Length > 0)
                        {
                            tagAttributes.Add("class");
                            tagAttributes.Add(classes);
                        }
                    }

                    var attributes = tagAttributes.ToArray();
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

            public override IRegion Wrap(IElement content)
            {
                return new BuiltRegion
                    {
                        WriteOpen = WriteOpen,
                        WriteClose = WriteClose,
                        Contents = content
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
                if (WriteOpen != null) WriteOpen(renderContext.Html);
                var result = Contents == null ? WriteResult.Continue() : Contents.WriteHtml(renderContext, dataContext);
                if (WriteClose != null) WriteClose(renderContext.Html);
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
