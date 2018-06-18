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
    internal class ComponentDefinition : IComponentDefinition
    {
        private readonly INameManager _nameManager;
        private readonly IAssetManager _assetManager;
        private readonly IComponentDependenciesFactory _componentDependenciesFactory;
        private readonly BuiltComponent _component;
        private readonly List<CssDefinition> _cssDefinitions;
        private readonly List<HtmlDefinition> _htmlToRender;

        private class CssDefinition
        {
            public string Selector;
            public string Style;
        }

        private class HtmlDefinition
        {
            public int Order;
            public string AssetName;
            public string DefaultHtml;
        }

        public ComponentDefinition(
            INameManager nameManager,
            IAssetManager assetManager,
            IComponentDependenciesFactory componentDependenciesFactory)
        {
            _nameManager = nameManager;
            _assetManager = assetManager;
            _componentDependenciesFactory = componentDependenciesFactory;
            _component = new BuiltComponent(componentDependenciesFactory);
            _cssDefinitions = new List<CssDefinition>();
            _htmlToRender = new List<HtmlDefinition>();
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

        IComponentDefinition IComponentDefinition.AssetDeployment(AssetDeployment assetDeployment)
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
            _htmlToRender.Add(new HtmlDefinition 
            { 
                AssetName = assetName, 
                DefaultHtml = defaultHtml 
            });
            return this;
        }

        IComponentDefinition IComponentDefinition.DeployCss(string cssSelector, string cssStyle)
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
            _component.StyleAssets = _cssDefinitions
                .Select(d =>
                {
                    var selector = d.Selector;
                    if (_component.Package == null)
                        selector = selector.Replace(".{ns}_", ".");
                    else
                        selector = selector.Replace(".{ns}_", "." + _component.Package.NamespaceName + "_");
                    return selector + " { " + d.Style + " }";
                })
                .Select(style =>
                {
                    Action<IHtmlWriter> writeAction = w => w.WriteLine(style);
                    return writeAction;
                })
                .ToList();

            _component.HtmlWriters = _htmlToRender
                .Select(d =>
                    {
                        Action<IRenderContext> action;

                        if (_component.Package == null)
                        {
                            action = rc =>
                            {
                                var localizedText = _assetManager.GetLocalizedText(rc, d.AssetName, d.DefaultHtml);
                                localizedText = localizedText.Replace("{ns}_", "");
                                rc.Html.WriteLine(localizedText);
                            };
                        }
                        else
                        {
                            action = rc =>
                            {
                                var localizedText = _assetManager.GetLocalizedText(rc, d.AssetName, d.DefaultHtml);
                                localizedText = localizedText.Replace("{ns}", _component.Package.NamespaceName);
                                rc.Html.WriteLine(localizedText);
                            };
                        }
                        return action;
                    })
                .ToList();

            _nameManager.Register(_component);

            return _component;
        }
    }
}
