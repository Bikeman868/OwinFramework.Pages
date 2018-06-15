using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Builders
{
    internal class ComponentBuilder : IComponentBuilder
    {
        private readonly INameManager _nameManager;
        private readonly IAssetManager _assetManager;

        public ComponentBuilder(
            INameManager nameManager,
            IAssetManager assetManager)
        {
            _nameManager = nameManager;
            _assetManager = assetManager;
        }

        IComponentDefinition IComponentBuilder.Component()
        {
            return new ComponentDefinition(_nameManager, _assetManager);
        }

        private class ComponentDefinition: IComponentDefinition
        {
            private readonly INameManager _nameManager;
            private readonly IAssetManager _assetManager;
            private readonly BuiltComponent _component;

            public ComponentDefinition(
                INameManager nameManager,
                IAssetManager assetManager)
            {
                _nameManager = nameManager;
                _assetManager = assetManager;
                _component = new BuiltComponent();
            }

            IComponentDefinition IComponentDefinition.Name(string name)
            {
                _component.Name = name;
                return this;
            }

            IComponentDefinition IComponentDefinition.PartOf(IPackage package)
            {
                _component.Package = package;
                return this;
            }

            IComponentDefinition IComponentDefinition.PartOf(string packageName)
            {
                _component.Package = _nameManager.ResolvePackage(packageName);

                if (_component.Package == null)
                    throw new ComponentBuilderException(
                        "Package names must be registered before components can refer to them. " +
                        "There is no registered package '" + packageName + "'");
                return this;
            }

            IComponentDefinition IComponentDefinition.AssetDeployment(Core.Enums.AssetDeployment assetDeployment)
            {
                _component.AssetDeployment = assetDeployment;
                return this;
            }

            IComponentDefinition IComponentDefinition.BindTo<T>()
            {
                return this;
            }

            IComponentDefinition IComponentDefinition.BindTo(Type dataType)
            {
                return this;
            }

            IComponentDefinition IComponentDefinition.DataContext(string dataContextName)
            {
                return this;
            }

            IComponentDefinition IComponentDefinition.DeployIn(IModule module)
            {
                _component.Module = module;
                return this;
            }

            IComponentDefinition IComponentDefinition.DeployIn(string moduleName)
            {
                _nameManager.AddResolutionHandler(() =>
                    {
                        _component.Module = _nameManager.ResolveModule(moduleName);
                    });
                return this;
            }

            IComponentDefinition IComponentDefinition.Render(string assetName, string defaultHtml)
            {
                if (_component.HtmlWriters == null)
                    _component.HtmlWriters = new List<Action<IRenderContext>>();

                _component.HtmlWriters.Add(rc => 
                    {
                        var localizedText = _assetManager.GetLocalizedText(rc, assetName, defaultHtml);
                        rc.Html.WriteLine(localizedText);
                    });

                return this;
            }

            IComponentDefinition IComponentDefinition.DeployCss(string cssSelector, string cssStyle)
            {
                if (string.IsNullOrEmpty(cssSelector))
                    throw new ComponentBuilderException("No CSS selector is specified for component CSS asset");

                if (string.IsNullOrEmpty(cssStyle))
                    throw new ComponentBuilderException("No CSS style is specified for component CSS asset");

                if (_component.StyleAssets == null)
                    _component.StyleAssets = new List<Action<IHtmlWriter>>();

                if (!cssStyle.EndsWith(";"))
                    cssStyle += ";";

                var style = cssSelector + " { " + cssStyle + " }";

                _component.StyleAssets.Add(w => w.WriteLine(style));

                return this;
            }

            IComponentDefinition IComponentDefinition.DeployFunction(string returnType, string functionName, string parameters, string functionBody)
            {
                if (string.IsNullOrEmpty(functionName))
                    throw new ComponentBuilderException("No function name is specified for component function asset");

                if (_component.FunctionAssets == null)
                    _component.FunctionAssets = new List<Action<IHtmlWriter>>();

                var declaration = (string.IsNullOrEmpty(returnType) ? "function " : returnType + " function ") + functionName + "(" + parameters + ") {";
                _component.FunctionAssets.Add(w =>
                    {
                        w.WriteLine(declaration);
                        w.IndentLevel++;
                    });

                foreach (var line in functionBody.Replace("\r", "").Split('\n'))
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        var l = line;
                        _component.FunctionAssets.Add(w => w.WriteLine(l));
                    }
                }
                _component.FunctionAssets.Add(w =>
                    {
                        w.IndentLevel--;
                        w.WriteLine("}");
                    });

                return this;
            }

            public IComponent Build()
            {
                _nameManager.Register(_component);
                return _component;
            }
        }

        private class BuiltComponent: Component
        {
            public List<Action<IRenderContext>> HtmlWriters;
            public List<Action<IHtmlWriter>> StyleAssets;
            public List<Action<IHtmlWriter>> FunctionAssets;

            public override IWriteResult WriteHtml(IRenderContext renderContext, IDataContext dataContext)
            {
                if (renderContext.IncludeComments)
                    renderContext.Html.WriteComment(
                        (string.IsNullOrEmpty(Name) ? "Unnamed" : Name) + 
                        (Package == null ? " component" : " component from the " + Package.Name + " package"));

                if (HtmlWriters != null)
                {
                    foreach (var writer in HtmlWriters)
                        writer(renderContext);
                }

                return WriteResult.Continue();
            }

            public override IWriteResult WriteStaticAssets(AssetType assetType, IHtmlWriter writer)
            {
                List<Action<IHtmlWriter>> assets = null;

                if (assetType == AssetType.Style)
                    assets = StyleAssets;
                else if (assetType == AssetType.Script)
                    assets = FunctionAssets;

                if (assets != null && assets.Count > 0)
                {
                    writer.WriteComment(
                            assetType + " assets for " +
                            (string.IsNullOrEmpty(Name) ? "unnamed" : Name) +
                            (Package == null ? " component" : " component from the " + Package.Name + " package"));

                    foreach (var asset in assets)
                        asset(writer);
                }

                return WriteResult.Continue();
            }
        }
    }
}
