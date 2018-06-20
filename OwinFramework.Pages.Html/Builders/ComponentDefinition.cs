﻿using System;
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
        private readonly List<FunctionDefinition> _functionDefinitions;
        private readonly List<CssDefinition> _cssDefinitions;
        private readonly List<HtmlDefinition> _htmlToRender;

        private class CssDefinition
        {
            public string Selector;
            public string Style;
        }

        private class FunctionDefinition
        {
            public string ReturnType;
            public string FunctionName;
            public string Parameters;
            public string Body;
            public bool IsPublic;
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
            _functionDefinitions = new List<FunctionDefinition>();
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

        IComponentDefinition IComponentDefinition.DeployFunction(string returnType, string functionName, string parameters, string functionBody, bool isPublic)
        {
            var functionDefinition = new FunctionDefinition 
            {
                ReturnType = returnType,
                FunctionName = functionName,
                Parameters = parameters,
                Body = functionBody,
                IsPublic = isPublic
            };

            _functionDefinitions.Add(functionDefinition);

            return this;
        }

        public IComponent Build()
        {
            _component.CssRules = _cssDefinitions
                .Select(d =>
                {
                    if (_component.Package == null)
                        d.Selector = d.Selector.Replace(".{ns}_", ".");
                    else
                        d.Selector = d.Selector.Replace(".{ns}_", "." + _component.Package.NamespaceName + "_");
                    return d;
                })
                .Select(d =>
                {
                    Action<ICssWriter> writeAction = w => w.WriteRule(d.Selector, d.Style);
                    return writeAction;
                })
                .ToList();

            _component.JavascriptFunctions = _functionDefinitions
                .Select(d =>
                {
                    Action<IJavascriptWriter> writeAction = w => w.WriteFunction(
                        d.FunctionName, d.Parameters, d.Body, d.ReturnType, _component.Package, d.IsPublic);
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
