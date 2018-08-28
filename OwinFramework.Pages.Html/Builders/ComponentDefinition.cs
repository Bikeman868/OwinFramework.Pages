using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Elements;
using OwinFramework.Pages.Html.Interfaces;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Builders
{
    internal class ComponentDefinition : IComponentDefinition
    {
        private readonly INameManager _nameManager;
        private readonly IAssetManager _assetManager;
        private readonly IHtmlHelper _htmlHelper;
        private readonly IFluentBuilder _fluentBuilder;
        private readonly Component _component;
        private readonly List<FunctionDefinition> _functionDefinitions;
        private readonly List<CssDefinition> _cssDefinitions;
        private readonly List<HtmlDefinition> _htmlToRender;

        private class HtmlDefinition
        {
            public int Order;
            public string AssetName;
            public string DefaultHtml;
        }

        public ComponentDefinition(
            Component component,
            INameManager nameManager,
            IAssetManager assetManager,
            IHtmlHelper htmlHelper,
            IFluentBuilder fluentBuilder,
            IPackage package)
        {
            _component = component;
            _nameManager = nameManager;
            _assetManager = assetManager;
            _htmlHelper = htmlHelper;
            _fluentBuilder = fluentBuilder;

            _cssDefinitions = new List<CssDefinition>();
            _functionDefinitions = new List<FunctionDefinition>();
            _htmlToRender = new List<HtmlDefinition>();

            if (package != null)
                _component.Package = package;
        }

        IComponentDefinition IComponentDefinition.Name(string name)
        {
            if (string.IsNullOrEmpty(name)) return this;

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
            if (string.IsNullOrEmpty(packageName)) return this;

            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolvePackageNames,
                (nm, c, n) => c.Package = nm.ResolvePackage(n),
                _component,
                packageName);

            return this;
        }

        IComponentDefinition IComponentDefinition.AssetDeployment(AssetDeployment assetDeployment)
        {
            _component.AssetDeployment = assetDeployment;
            return this;
        }

        IComponentDefinition IComponentDefinition.BindTo<T>(string scopeName)
        {
            var dataConsumer = _component as IDataConsumer;
            if (dataConsumer == null)
                throw new ComponentBuilderException("This component is not a consumer of data");

            dataConsumer.HasDependency<T>(scopeName);

            return this;
        }

        IComponentDefinition IComponentDefinition.BindTo(Type dataType, string scopeName)
        {
            if (dataType == null)
                throw new ComponentBuilderException("To define data binding you must specify the type of data to bind");

            var dataConsumer = _component as IDataConsumer;
            if (dataConsumer == null)
                throw new ComponentBuilderException("This component is not a consumer of data");

            dataConsumer.HasDependency(dataType, scopeName);

            return this;
        }

        IComponentDefinition IComponentDefinition.DataProvider(string dataProviderName)
        {
            if (string.IsNullOrEmpty(dataProviderName)) return this;

            var dataConsumer = _component as IDataConsumer;
            if (dataConsumer == null)
                throw new ComponentBuilderException("This component is not a consumer of data");

            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolveElementReferences,
                (nm, c, n) => c.HasDependency(nm.ResolveDataProvider(n)),
                dataConsumer,
                dataProviderName);

            return this;
        }

        IComponentDefinition IComponentDefinition.DataProvider(IDataProvider dataProvider)
        {
            var dataConsumer = _component as IDataConsumer;
            if (dataConsumer == null)
                throw new ComponentBuilderException("This component is not a consumer of data");

            dataConsumer.HasDependency(dataProvider);

            return this;
        }

        IComponentDefinition IComponentDefinition.DeployIn(IModule module)
        {
            _component.Module = module;
            return this;
        }

        IComponentDefinition IComponentDefinition.DeployIn(string moduleName)
        {
            if (string.IsNullOrEmpty(moduleName)) return this;

            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolveElementReferences,
                (nm, c, n) => c.Module = nm.ResolveModule(n),
                _component,
                moduleName);
            
            return this;
        }

        IComponentDefinition IComponentDefinition.Render(string assetName, string defaultHtml)
        {
            if (string.IsNullOrEmpty(assetName))
                throw new ComponentBuilderException("No asset name specified for component html");

            if (string.IsNullOrEmpty(defaultHtml)) return this;

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

        IComponentDefinition IComponentDefinition.NeedsComponent(string componentName)
        {
            if (string.IsNullOrEmpty(componentName))
                throw new ComponentBuilderException("No component name provided in component dependency");

            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolveElementReferences,
                (nm, c, n) => c.NeedsComponent(nm.ResolveComponent(n)),
                _component,
                componentName);

            return this;
        }

        IComponentDefinition IComponentDefinition.NeedsComponent(IComponent component)
        {
            if (ReferenceEquals(component, null))
                throw new ComponentBuilderException("Null component reference for dependent component");

            _component.NeedsComponent(component);

            return this;
        }

        public IComponent Build()
        {
            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolveAssetNames,
                () => 
                { 
                    _component.CssRules = _cssDefinitions
                        .Select(d =>
                        {
                            d.Selector = _htmlHelper.NamespaceCssSelector(d.Selector, _component.Package);
                            return d;
                        })
                        .Select(d =>
                        {
                            Action<ICssWriter> writeAction = w => w.WriteRule(d.Selector, d.Style);
                            return writeAction;
                        })
                        .ToArray();

                    _component.JavascriptFunctions = _functionDefinitions
                        .Select(d =>
                        {
                            Action<IJavascriptWriter> writeAction = w => w.WriteFunction(
                                d.FunctionName, d.Parameters, d.Body, d.ReturnType, _component.Package, d.IsPublic);
                            return writeAction;
                        })
                        .ToArray();

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
                        .ToArray();
                });

            _fluentBuilder.Register(_component);
            return _component;
        }
    }
}
