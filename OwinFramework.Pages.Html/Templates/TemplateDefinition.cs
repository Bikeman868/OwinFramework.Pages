using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Core.Interfaces.Templates;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Templates
{
    internal class TemplateDefinition : ITemplateDefinition
    {
        private readonly INameManager _nameManager;
        private readonly IAssetManager _assetManager;
        private readonly Template _template;
        private readonly RenderActionList _headActions;
        private readonly RenderActionList _scriptActions;
        private readonly RenderActionList _styleActions;
        private readonly RenderActionList _bodyActions;
        private readonly RenderActionList _initializationActions;
        private readonly Stack<Element> _elementStack;
        private readonly Stack<Repeat> _repeatStack;
        private readonly JavascriptActionList _staticJavascriptActions;
        private readonly CssActionList _staticCssActions;

        private IPackage _package;
        private Element _element;
        private Repeat _repeat;

        private RenderActionList HeadActions { get { return _headActions; } }
        private RenderActionList ScriptActions { get { return _scriptActions; } }
        private RenderActionList StyleActions { get { return _styleActions; } }
        private RenderActionList BodyActions { get { return _repeat ?? _bodyActions; } }
        private RenderActionList InitializationActions { get { return _initializationActions; } }
        private JavascriptActionList StaticJavascriptActions { get { return _staticJavascriptActions; } }
        private CssActionList StaticCssActions { get { return _staticCssActions; } }

        private class Element
        {
            public string Tag;
            public List<string> Attributes = new List<string>();
        }

        private class JavascriptActionList
        {
            protected readonly List<Action<IJavascriptWriter>> _actions = new List<Action<IJavascriptWriter>>();

            public void Add(Action<IJavascriptWriter> action)
            {
                _actions.Add(action);
            }

            public void ToTemplate(Template template)
            {
                if (_actions.Count > 0)
                    template.AddStaticJavascript(_actions);
            }
        }

        private class CssActionList
        {
            protected readonly List<Action<ICssWriter>> _actions = new List<Action<ICssWriter>>();

            public void Add(Action<ICssWriter> action)
            {
                _actions.Add(action);
            }

            public void ToTemplate(Template template)
            {
                if (_actions.Count > 0)
                    template.AddStaticCss(_actions);
            }
        }

        private abstract class RenderActionList
        {
            protected readonly List<Action<IRenderContext>> _renderActions = new List<Action<IRenderContext>>();

            public void Add(Action<IRenderContext> action)
            {
                _renderActions.Add(action);
            }

            public void AddPrior(Action<IRenderContext> action)
            {
                if (_renderActions.Count > 0)
                {
                    _renderActions.Add(_renderActions[_renderActions.Count - 1]);
                    _renderActions[_renderActions.Count - 2] = action;
                }
                else
                {
                    Add(action);
                }
            }

            public Action<Action<IRenderContext>> AddPlaceholder()
            {
                var index = _renderActions.Count;
                _renderActions.Add(null);

                return a => _renderActions[index] = a;
            }

            public abstract void ToTemplate(Template template);
        }

        private class BodyActionList : RenderActionList
        {
            public override void ToTemplate(Template template)
            {
                if (_renderActions.Count > 0)
                    template.Add(_renderActions);
            }
        }

        private class HeadActionList : RenderActionList
        {
            public override void ToTemplate(Template template)
            {
                if (_renderActions.Count > 0)
                    template.AddHead(_renderActions);
            }
        }

        private class ScriptActionList : RenderActionList
        {
            public override void ToTemplate(Template template)
            {
                if (_renderActions.Count > 0)
                    template.AddScript(_renderActions);
            }
        }

        private class StyleActionList : RenderActionList
        {
            public override void ToTemplate(Template template)
            {
                if (_renderActions.Count > 0)
                    template.AddStyle(_renderActions);
            }
        }

        private class InitializationActionList : RenderActionList
        {
            public override void ToTemplate(Template template)
            {
                if (_renderActions.Count > 0)
                    template.AddInitialization(_renderActions);
            }
        }

        private class Repeat : BodyActionList
        {
            private readonly Type _repeatType;
            private readonly Type _listType;
            private readonly string _listScope;
            private readonly string _repeatScope;

            private Action<IRenderContext>[] _actions;

            public Repeat(Type repeatType, string listScope, string repeatScope)
            {
                _repeatType = repeatType;
                _listScope = listScope;
                _repeatScope = repeatScope;

                _listType = typeof(IList<>).MakeGenericType(repeatType);
            }

            public void Enact(IRenderContext context)
            {
                if (_actions == null)
                {
                    // We are copying the list to an array here because arrays
                    // are thread safe and List<T> is not
                    lock(this)
                    {
                        if (_actions == null)
                        {
                            _actions = _renderActions.ToArray();
                            _renderActions.Clear();
                        }
                    }
                }

                var list = context.Data.Get(_listType, _listScope) as IEnumerable;
                if (!ReferenceEquals(list, null))
                {
                    foreach (var item in list)
                    {
                        context.Data.Set(_repeatType, item, _repeatScope);
                        for (var i = 0; i < _actions.Length; i++)
                            _actions[i](context);
                    }
                }
            }

            public bool IsRepeaterOf(Type dataType, string scopeName)
            {
                if (dataType != _repeatType) return false;
                if (string.IsNullOrEmpty(scopeName)) return true;
                return string.Equals(scopeName, _repeatScope);
            }
        }

        public TemplateDefinition(
            INameManager nameManager,
            IAssetManager assetManager,
            IDataConsumerFactory dataConsumerFactory)
        {
            _nameManager = nameManager;
            _assetManager = assetManager;

            _headActions = new HeadActionList();
            _scriptActions = new ScriptActionList();
            _styleActions = new StyleActionList();
            _bodyActions = new BodyActionList();
            _initializationActions = new InitializationActionList();
            _staticJavascriptActions = new JavascriptActionList();
            _staticCssActions = new CssActionList();

            _elementStack = new Stack<Element>();
            _repeatStack = new Stack<Repeat>();
            _template = new Template(dataConsumerFactory);
        }

        public ITemplateDefinition PartOf(string packageName)
        {
            _nameManager.AddResolutionHandler(NameResolutionPhase.ResolvePackageNames, nm =>
                {
                    _package = nm.ResolvePackage(packageName);
                    ((ITemplate)_template).Package = _package;
                });
            return this;
        }

        public ITemplateDefinition PartOf(IPackage package)
        {
            _package = package;
            ((ITemplate)_template).Package = package;
            return this;
        }

        public ITemplateDefinition DeployIn(string moduleName)
        {
            _nameManager.AddResolutionHandler(NameResolutionPhase.ResolveElementReferences, nm =>
                {
                    ((ITemplate)_template).Module = nm.ResolveModule(moduleName);
                });
            return this;
        }

        public ITemplateDefinition DeployIn(IModule module)
        {
            ((ITemplate)_template).Module = module;
            return this;
        }

        public ITemplateDefinition AssetDeployment(AssetDeployment assetDeployment)
        {
            ((ITemplate)_template).AssetDeployment = assetDeployment;
            return this;
        }

        public ITemplateDefinition AddHtml(string html)
        {
            BodyActions.Add(r => r.Html.Write(html));
            return this;
        }

        public ITemplateDefinition AddElement(string tag, string content, params string[] attributePairs)
        {
            BodyActions.Add(r => r.Html.WriteElement(tag, content, attributePairs));
            return this;
        }

        public ITemplateDefinition AddSelfClosingElement(string tag, params string[] attributePairs)
        {
            BodyActions.Add(r => r.Html.WriteOpenTag(tag, true, attributePairs));
            return this;
        }

        public ITemplateDefinition AddElementOpen(string tag, params string[] attributePairs)
        {
            WriteElementOpenTag(tag, attributePairs);
            return this;
        }

        public ITemplateDefinition SetElementAttribute(string attributeName, Type dataType, string propertyName, IDataFieldFormatter dataFormatter = null, string scopeName = null)
        {
            if (_element == null)
                throw new TemplateBuilderException("You cannot set element attributes " +
                    "here since there is no current element. Call AddElementOpen() before setting element attributes.");

            _element.Attributes.Add(attributeName);
            _element.Attributes.Add(attributeName);

            var element = _element;
            var attributeIndex = _element.Attributes.Count - 1;

            var property = dataType.GetProperties().FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));
            if (property == null)
                throw new TemplateBuilderException("Type " + dataType.DisplayName() + " does not have a public '" + propertyName + "' property");

            BodyActions.AddPrior(r =>
                {
                    var data = r.Data.Get(dataType, scopeName);
                    var propertyValue = property.GetValue(data, null);
                    var formattedValue = dataFormatter == null ? propertyValue.ToString() : dataFormatter.Format(property, propertyValue);
                    element.Attributes[attributeIndex] = formattedValue;
                });

            var dataConsumer = _template as IDataConsumer;
            if (dataConsumer != null)
                dataConsumer.HasDependency(dataType, scopeName);

            return this;
        }

        public ITemplateDefinition AddElementClose()
        {
            WriteElementCloseTag();
            return this;
        }

        public ITemplateDefinition AddLineBreak()
        {
            BodyActions.Add(r => r.Html.WriteLine());
            return this;
        }

        public ITemplateDefinition AddLayout(string layoutName)
        {
            var placeholder = BodyActions.AddPlaceholder();

            _nameManager.AddResolutionHandler(NameResolutionPhase.ResolveElementReferences, nm =>
            {
                var e = nm.ResolveLayout(layoutName, _package);
                placeholder(r => e.WritePageArea(r, PageArea.Body, (rc, pa, rn) => WriteResult.Continue())); // TODO: Write layout regions
                AddConsumerNeeds(e as IDataConsumer);
            });

            return this;
        }

        public ITemplateDefinition AddLayout(ILayout layout)
        {
            BodyActions.Add(r => layout.WritePageArea(r, PageArea.Body, (rc, pa, rn) => WriteResult.Continue())); // TODO: Write layout regions
            AddConsumerNeeds(layout as IDataConsumer);

            return this;
        }

        public ITemplateDefinition AddRegion(string regionName)
        {
            var placeholder = BodyActions.AddPlaceholder();

            _nameManager.AddResolutionHandler(NameResolutionPhase.ResolveElementReferences, nm =>
            {
                var e = nm.ResolveRegion(regionName, _package);
                placeholder(r => e.WritePageArea(r, PageArea.Body, null, (rc, pa) => WriteResult.Continue())); // TODO: Write region
                AddConsumerNeeds(e as IDataConsumer);
            });

            return this;
        }

        public ITemplateDefinition AddRegion(IRegion region)
        {
            BodyActions.Add(r => region.WritePageArea(r, PageArea.Body, null, (rc, pa) => WriteResult.Continue())); // TODO: Write region
            AddConsumerNeeds(region as IDataConsumer);

            return this;
        }

        public ITemplateDefinition AddComponent(string componentName)
        {
            var placeholder = BodyActions.AddPlaceholder();

            _nameManager.AddResolutionHandler(NameResolutionPhase.ResolveElementReferences, nm =>
                {
                    var e = nm.ResolveComponent(componentName, _package);
                    placeholder(r => e.WritePageArea(r, PageArea.Body));
                    AddConsumerNeeds(e as IDataConsumer);
                });

            return this;
        }

        public ITemplateDefinition AddComponent(IComponent component)
        {
            BodyActions.Add(r => component.WritePageArea(r, PageArea.Body));
            AddConsumerNeeds(component as IDataConsumer);

            return this;
        }

        public ITemplateDefinition AddTemplate(string templatePath)
        {
            _nameManager.AddResolutionHandler(NameResolutionPhase.ResolveElementReferences, nm =>
            {
                AddConsumerNeeds(nm.ResolveTemplate(templatePath) as IDataConsumer);
            });

            HeadActions.Add(r => 
            {
                var t = _nameManager.ResolveTemplate(templatePath);
                if (t != null) t.WritePageArea(r, PageArea.Head);
            });

            ScriptActions.Add(r =>
            {
                var t = _nameManager.ResolveTemplate(templatePath);
                if (t != null) t.WritePageArea(r, PageArea.Scripts);
            });

            StyleActions.Add(r =>
            {
                var t = _nameManager.ResolveTemplate(templatePath);
                if (t != null) t.WritePageArea(r, PageArea.Styles);
            });

            BodyActions.Add(r =>
            {
                var t = _nameManager.ResolveTemplate(templatePath);
                if (t != null) t.WritePageArea(r, PageArea.Body);
            });

            InitializationActions.Add(r =>
            {
                var t = _nameManager.ResolveTemplate(templatePath);
                if (t != null) t.WritePageArea(r, PageArea.Initialization);
            });

            return this;
        }

        public ITemplateDefinition AddTemplate(ITemplate template)
        {
            HeadActions.Add(r => template.WritePageArea(r, PageArea.Head));
            ScriptActions.Add(r => template.WritePageArea(r, PageArea.Scripts));
            StyleActions.Add(r => template.WritePageArea(r, PageArea.Styles));
            BodyActions.Add(r => template.WritePageArea(r, PageArea.Body));
            InitializationActions.Add(r => template.WritePageArea(r, PageArea.Initialization));

            AddConsumerNeeds(template as IDataConsumer);

            return this;
        }

        public ITemplateDefinition RepeatStart<T>(string scopeName, string listScopeName)
        {
            return RepeatStart(typeof(T), scopeName, listScopeName);
        }

        public ITemplateDefinition AddData(Type dataType, string scopeName = null)
        {
            // TODO: Figure out how to do this
            BodyActions.Add(r =>
                {
                    // r.Data.Set(dataType, value, scopeName);
                });
            return this;
        }

        public ITemplateDefinition AddData<T>(string scopeName = null)
        {
            // TODO: Figure out how to do this
            BodyActions.Add(r =>
            {
                // r.Data.Set(typeof(T), value, scopeName);
            });
            return this;
        }

        public ITemplateDefinition ExtractProperty(Type dataType, string propertyName, string scopeName = null, string propertyScopeName = null)
        {
            var property = dataType.GetProperties().FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));
            if (property == null)
                throw new TemplateBuilderException("Type " + dataType.DisplayName() + " does not have a public '" + propertyName + "' property");

            return ExtractProperty(dataType, property, scopeName, propertyScopeName);
        }

        public ITemplateDefinition ExtractProperty<T>(Expression<Func<T, object>> propertyExpression, string scopeName = null, string propertyScopeName = null)
        {
            var expression = (MemberExpression)propertyExpression.Body;
            var property = (PropertyInfo)expression.Member;

            return ExtractProperty(typeof(T), property, scopeName, propertyScopeName);
        }

        private ITemplateDefinition ExtractProperty(Type dataType, PropertyInfo property, string scopeName = null, string propertyScopeName = null)
        {
            BodyActions.Add(r =>
            {
                var obj = r.Data.Get(dataType, propertyScopeName);
                var value = property.GetValue(obj, null);
                r.Data.Set(property.PropertyType, value, scopeName);
            });
            return this;
        }

        public ITemplateDefinition RepeatStart(Type dataTypeToRepeat, string scopeName, string listScopeName)
        {
            var repeat = new Repeat(dataTypeToRepeat, listScopeName, scopeName);
            BodyActions.Add(repeat.Enact);

            if (_repeat != null)
                _repeatStack.Push(_repeat);

            _repeat = repeat;

            return this;
        }

        public ITemplateDefinition RepeatEnd()
        {
            _repeat = _repeatStack.Count > 0 ? _repeatStack.Pop() : null;
            return this;
        }

        public ITemplateDefinition AddDataField<T>(Expression<Func<T, object>> propertyExpression, IDataFieldFormatter dataFormatter = null, string scopeName = null)
        {
            var expression = (MemberExpression)propertyExpression.Body;
            var property = (PropertyInfo)expression.Member;

            return AddDataField(typeof(T), property, dataFormatter, scopeName);
        }

        public ITemplateDefinition AddDataField<T>(Func<T, string> formatFunc, string scopeName = null)
        {
            BodyActions.Add(r =>
            {
                var data = r.Data.Get<T>(scopeName);
                var formattedValue = formatFunc(data);
                r.Html.WriteText(formattedValue);
            });

            AddDependency(typeof(T), scopeName);

            return this;
        }

        public ITemplateDefinition AddDataField(Type dataType, string propertyName, IDataFieldFormatter dataFormatter = null, string scopeName = null)
        {
            var property = dataType.GetProperties().FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));
            if (property == null)
                throw new TemplateBuilderException("Type " + dataType.DisplayName() + " does not have a public '" + propertyName + "' property");

            return AddDataField(dataType, property, dataFormatter, scopeName);
        }

        private ITemplateDefinition AddDataField(Type dataType, PropertyInfo property, IDataFieldFormatter dataFormatter = null, string scopeName = null)
        {
            BodyActions.Add(r =>
            {
                string formattedValue;

                var data = r.Data.Get(dataType, scopeName);
                if (data == null)
                {
                    if (dataFormatter == null)
                        formattedValue = null;
                    else
                        formattedValue = dataFormatter.Format(property, null);
                }
                else
                {
                    var propertyValue = property.GetValue(data, null);

                    if (dataFormatter == null)
                        formattedValue = propertyValue == null ? null : propertyValue.ToString();
                    else
                        formattedValue = dataFormatter.Format(property, propertyValue);
                }

                if (!string.IsNullOrEmpty(formattedValue))
                    r.Html.WriteText(formattedValue);
            });

            AddDependency(dataType, scopeName);

            return this;
        }

        public ITemplateDefinition AddText(string assetName, string defaultText, bool isPreFormatted)
        {
            BodyActions.Add(r =>
            {
                var html = r.Html;

                var localizedString = _assetManager.GetLocalizedText(r, assetName, defaultText);
                if (html.IncludeComments && !isPreFormatted)
                {
                    localizedString = localizedString.Replace('\r', '\n');
                    var endsWithLineBreak = localizedString.EndsWith("\n");

                    var lines = localizedString
                        .Split('\n')
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToList();
                    for (var i = 0; i < lines.Count; i++)
                    {
                        var line = lines[i];
                        if (i < lines.Count - 1 || endsWithLineBreak)
                            html.WriteTextLine(line);
                        else
                            html.WriteText(line);
                    }
                }
                else
                {
                    if (isPreFormatted)
                        html.WritePreformatted(localizedString);
                    else
                        html.WriteText(localizedString);
                }
            });
            return this;
        }

        public ITemplateDefinition AddHeadLine(string html)
        {
            HeadActions.Add(r =>
                {
                    r.Html.Write(html);
                    r.Html.WriteLine();
                });
            return this;
        }

        public ITemplateDefinition AddScriptLine(string javaScript)
        {
            ScriptActions.Add(r =>
                {
                    r.Html.Write(javaScript);
                    r.Html.WriteLine();
                });
            return this;
        }

        public ITemplateDefinition AddStyleLine(string css)
        {
            StyleActions.Add(r =>
                {
                    r.Html.Write(css);
                    r.Html.WriteLine();
                });
            return this;
        }

        public ITemplateDefinition AddInitializationLine(string javaScript)
        {
            InitializationActions.Add(r =>
                {
                    r.Html.Write(javaScript);
                    r.Html.WriteLine();
                });
            return this;
        }

        public ITemplateDefinition AddStaticJavascript(string rawJavascript)
        {
            StaticJavascriptActions.Add(w =>
            {
                w.WriteLineRaw(rawJavascript);
            });
            return this;
        }

        public ITemplateDefinition AddStaticCss(string css)
        {
            StaticCssActions.Add(w =>
            {
                w.WriteLineRaw(css);
            });
            return this;
        }

        public ITemplate Build()
        {
            HeadActions.ToTemplate(_template);
            ScriptActions.ToTemplate(_template);
            StyleActions.ToTemplate(_template);
            BodyActions.ToTemplate(_template);
            InitializationActions.ToTemplate(_template);
            StaticCssActions.ToTemplate(_template);
            StaticJavascriptActions.ToTemplate(_template);
            return _template;
        }

        #region Private methods

        private void WriteElementOpenTag(string tag, params string[] attributePairs)
        {
            if (_element != null)
                _elementStack.Push(_element);

            _element = new Element { Tag = tag };

            if (attributePairs != null && attributePairs.Length > 0)
            {
                if (attributePairs.Length % 2 != 0)
                    throw new TemplateBuilderException("Element attributes must be in pairs " +
                        "of name and value, but an odd number of parameters were passed");

                _element.Attributes = attributePairs.ToList();
            }

            var element = _element;
            BodyActions.Add(r => r.Html.WriteOpenTag(element.Tag, element.Attributes.ToArray()));
        }

        private void WriteElementCloseTag()
        {
            var element = _element;
            BodyActions.Add(r => r.Html.WriteCloseTag(element.Tag));

            _element = _elementStack.Count > 0 ? _elementStack.Pop() : null;
        }

        private void AddConsumerNeeds(IDataConsumer otherDataConsumer)
        {
            var thisDataConsumer = _template as IDataConsumer;

            if (otherDataConsumer == null || thisDataConsumer == null)
                return;

            var needs = otherDataConsumer.GetConsumerNeeds();
            if (needs == null)
                return;

            if (needs.DataDependencies != null)
            {
                foreach (var dependency in needs.DataDependencies)
                    thisDataConsumer.HasDependency(dependency.DataType, dependency.ScopeName);
            }

            if (needs.DataSupplyDependencies != null)
            {
                foreach (var dataSupply in needs.DataSupplyDependencies)
                    thisDataConsumer.HasDependency(dataSupply);
            }

            if (needs.DataSupplierDependencies != null)
            {
                foreach (var dataSupplier in needs.DataSupplierDependencies)
                    thisDataConsumer.HasDependency(dataSupplier.Item1, dataSupplier.Item2);
            }
        }

        private void AddDependency(Type dataType, string scopeName)
        {
            var dataConsumer = _template as IDataConsumer;
            if (ReferenceEquals(dataConsumer, null))
                return;

            if (!ReferenceEquals(_repeat, null) && _repeat.IsRepeaterOf(dataType, scopeName))
                return;

            if (_repeatStack.Any(r => r.IsRepeaterOf(dataType, scopeName)))
                return;

            dataConsumer.HasDependency(dataType, scopeName);
        }

        #endregion


    }
}
