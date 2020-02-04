using System;
using System.Collections.Generic;
using System.Linq;
using dotless.Core.configuration;
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
        private readonly List<LessDefinition> _lessDefinitions;
        private readonly List<HtmlDefinition> _headHtmlToRender;
        private readonly List<HtmlDefinition> _bodyHtmlToRender;
        private readonly List<HtmlDefinition> _initializationHtmlToRender;

        private string _javaScript;

        private class HtmlDefinition
        {
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
            _lessDefinitions = new List<LessDefinition>();
            _functionDefinitions = new List<FunctionDefinition>();
            _headHtmlToRender = new List<HtmlDefinition>();
            _bodyHtmlToRender = new List<HtmlDefinition>();
            _initializationHtmlToRender = new List<HtmlDefinition>();

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

        IComponentDefinition IComponentDefinition.RenderHead(string assetName, string defaultHtml)
        {
            if (string.IsNullOrEmpty(assetName))
                throw new ComponentBuilderException("No asset name specified for component head html");

            if (string.IsNullOrEmpty(defaultHtml)) return this;

            _headHtmlToRender.Add(new HtmlDefinition 
            { 
                AssetName = assetName, 
                DefaultHtml = defaultHtml 
            });

            return this;
        }

        IComponentDefinition IComponentDefinition.Render(string assetName, string defaultHtml)
        {
            if (string.IsNullOrEmpty(assetName))
                throw new ComponentBuilderException("No asset name specified for component html");

            if (string.IsNullOrEmpty(defaultHtml)) return this;

            _bodyHtmlToRender.Add(new HtmlDefinition
            {
                AssetName = assetName,
                DefaultHtml = defaultHtml
            });

            return this;
        }

        IComponentDefinition IComponentDefinition.RenderInitialization(string assetName, string defaultHtml)
        {
            if (string.IsNullOrEmpty(assetName))
                throw new ComponentBuilderException("No asset name specified for component initialization html");

            if (string.IsNullOrEmpty(defaultHtml)) return this;

            _initializationHtmlToRender.Add(new HtmlDefinition
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

        IComponentDefinition IComponentDefinition.DeployLess(string lessStyles)
        {
            if (!string.IsNullOrWhiteSpace(lessStyles))
            {
                var lessDefinition = new LessDefinition
                {
                    LessStyles = lessStyles.Trim()
                };

                _lessDefinitions.Add(lessDefinition);
            }
            return this;
        }

        IComponentDefinition IComponentDefinition.DeployScript(string script)
        {
            _javaScript = _javaScript == null ? script : _javaScript + "\n" + script;
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
                (nm, c, n) => c.NeedsComponent(nm.ResolveComponent(n, c.Package)),
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

        IComponent IComponentDefinition.Build()
        {
            _nameManager.AddResolutionHandler(
                NameResolutionPhase.ResolveAssetNames,
                () =>
                {
                    var css = new List<Action<ICssWriter>>();

                    if (_lessDefinitions != null && _lessDefinitions.Count > 0)
                    {
                        var aggregate = _lessDefinitions.Aggregate(string.Empty, (a, s) => a + _htmlHelper.NamespaceCssSelector(s.LessStyles, _component.Package) + '\n');
                        var dotlessConfig = new DotlessConfiguration { MinifyOutput = true };
                        var less = dotless.Core.Less.Parse(aggregate, dotlessConfig);

                        foreach( var line in less.Replace("\r", "").Split('\n'))
                        {
                            var trimmed = line.Trim();
                            if (trimmed.Length > 0)
                            {
                                Action<ICssWriter> writeAction = w => w.WriteLineRaw(trimmed);
                                css.Add(writeAction);
                            }
                        }
                    }

                    if (_cssDefinitions != null && _cssDefinitions.Count > 0)
                    {
                        css.AddRange(_cssDefinitions
                            .Select(d =>
                            {
                                d.Selector = _htmlHelper.NamespaceCssSelector(d.Selector, _component.Package);
                                Action<ICssWriter> writeAction = w => w.WriteRule(d.Selector, d.Style);
                                return writeAction;
                            }));
                    }

                    if (_component.CssRules == null)
                        _component.CssRules = css.ToArray();
                    else
                        _component.CssRules = _component.CssRules.Concat(css).ToArray();

                    var javascriptWriters = new List<Action<IJavascriptWriter>>();

                    if (!string.IsNullOrEmpty(_javaScript))
                    {
                        javascriptWriters.Add(w => w.WriteLineRaw(_javaScript, _component.Package));
                    }

                    if (_functionDefinitions != null && _functionDefinitions.Count > 0)
                    {
                        javascriptWriters.AddRange(_functionDefinitions
                            .Select(d =>
                            {
                                Action<IJavascriptWriter> writeAction = w => w.WriteFunction(
                                    d.FunctionName, d.Parameters, d.Body, d.ReturnType, _component.Package, d.IsPublic);
                                return writeAction;
                            }));
                    }

                    if (_component.JavascriptFunctions == null)
                        _component.JavascriptFunctions = javascriptWriters.ToArray();
                    else
                        _component.JavascriptFunctions = _component.JavascriptFunctions.Concat(javascriptWriters).ToArray();

                    if (_headHtmlToRender.Count > 0)
                    {
                        _component.HeadWriters = _headHtmlToRender
                            .Select(GetHtmlAction)
                            .ToArray();
                    }

                    if (_bodyHtmlToRender.Count > 0)
                    {
                        _component.BodyWriters = _bodyHtmlToRender
                            .Select(GetHtmlAction)
                            .ToArray();
                    }

                    if (_initializationHtmlToRender.Count > 0)
                    {
                        _component.InitializationWriters = _initializationHtmlToRender
                            .Select(GetHtmlAction)
                            .ToArray();
                    }
                });

            _fluentBuilder.Register(_component);
            return _component;
        }

        private Action<IRenderContext> GetHtmlAction(HtmlDefinition d)
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
        }
    }
}
