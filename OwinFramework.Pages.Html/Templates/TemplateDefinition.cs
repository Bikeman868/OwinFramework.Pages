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

        private class RenderActionList
        {
            protected readonly List<Action<IRenderContext>> _renderActions = new List<Action<IRenderContext>>();
            private readonly PageArea _pageArea;

            public RenderActionList(PageArea pageArea)
            {
                _pageArea = pageArea;
            }

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

            public void ToTemplate(Template template)
            {
                if (_renderActions.Count < 1) return;

                switch (_pageArea)
                {
                    case PageArea.Head:
                        template.AddHead(_renderActions);
                        break;
                    case PageArea.Scripts:
                        template.AddScript(_renderActions);
                        break;
                    case PageArea.Styles:
                        template.AddStyle(_renderActions);
                        break;
                    case PageArea.Initialization:
                        template.AddInitialization(_renderActions);
                        break;
                    default:
                        template.Add(_renderActions);
                        break;
                }
            }
        }

        private class Repeat: RenderActionList
        {
            public Type RepeatType { get; private set; }
            public string ListScope { get; private set; }
            public string RepeatScope { get; private set; }

            protected Action<IRenderContext>[] _actions;

            public Repeat(PageArea pageArea, Type repeatType, string listScope, string repeatScope)
                : base(pageArea)
            {
                RepeatType = repeatType;
                ListScope = listScope;
                RepeatScope = repeatScope;
            }

            public bool IsRepeaterOf(Type dataType, string scopeName)
            {
                if (dataType != RepeatType) return false;
                if (string.IsNullOrEmpty(scopeName)) return true;
                return string.Equals(scopeName, RepeatScope);
            }

            public virtual void Enact(IRenderContext context)
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
            }
        }

        private class RepeatList : Repeat
        {
            public Type ListType { get; private set; }

            public RepeatList(PageArea pageArea, Type repeatType, string listScope, string repeatScope)
                : base(pageArea, repeatType, listScope, repeatScope)
            {
                ListType = typeof(IList<>).MakeGenericType(repeatType);
            }

            public override void Enact(IRenderContext context)
            {
                base.Enact(context);

                var list = context.Data.Get(ListType, ListScope) as IEnumerable;

                if (!ReferenceEquals(list, null))
                {
                    foreach (var item in list)
                    {
                        context.Data.Set(RepeatType, item, RepeatScope);
                        for (var i = 0; i < _actions.Length; i++)
                            _actions[i](context);
                    }
                }
            }
        }

        private class RepeatOnce : Repeat
        {
            public PropertyInfo Property { get; private set; }
            public ITruthyEvaluator TruthyEvaluator { get; private set; }

            public RepeatOnce(
                PageArea pageArea,
                Type repeatType, 
                PropertyInfo property, 
                ITruthyEvaluator truthyEvaluator , 
                string listScope, 
                string repeatScope)
                : base(pageArea, repeatType, listScope, repeatScope)
            {
                Property = property;
                TruthyEvaluator = truthyEvaluator;
            }

            public override void Enact(IRenderContext context)
            {
                base.Enact(context);

                var theObject = context.Data.Get(RepeatType, ListScope, false);

                if (TruthyEvaluator.IsTruthy(Property, theObject))
                {
                    context.Data.Set(RepeatType, theObject, RepeatScope);
                    for (var i = 0; i < _actions.Length; i++)
                        _actions[i](context);
                }
            }
        }

        public TemplateDefinition(
            INameManager nameManager,
            IAssetManager assetManager,
            IDataConsumerFactory dataConsumerFactory)
        {
            _nameManager = nameManager;
            _assetManager = assetManager;

            _headActions = new RenderActionList(PageArea.Head);
            _scriptActions = new RenderActionList(PageArea.Scripts);
            _styleActions = new RenderActionList(PageArea.Styles);
            _bodyActions = new RenderActionList(PageArea.Body);
            _initializationActions = new RenderActionList(PageArea.Initialization);
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

        public ITemplateDefinition AddHtml(PageArea pageArea, string html)
        {
            if (string.IsNullOrEmpty(html))
                return this;

            if (html.IndexOf('\n') >= 0)
            {
                var lines = html.Split('\n');
                if (lines.Length == 1)
                {
                    var line = lines[0];
                    ActionList(pageArea).Add(r => r.Html.WriteLine(line));
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(lines[lines.Length - 1]))
                    {
                        lines = lines.Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
                        ActionList(pageArea).Add(r =>
                        {
                            foreach (var line in lines)
                                r.Html.WriteLine(line);
                        });
                    }
                    else
                    {
                        lines = lines.Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
                        ActionList(pageArea).Add(r =>
                        {
                            for (var i = 0; i < lines.Length - 1; i++)
                                r.Html.WriteLine(lines[i]);
                            r.Html.Write(lines[lines.Length - 1]);
                        });
                    }
                }
            }
            else
            {
                ActionList(pageArea).Add(r => r.Html.Write(html));
            }
            return this;
        }

        public ITemplateDefinition AddElement(PageArea pageArea, string tag, string content, params string[] attributePairs)
        {
            ActionList(pageArea).Add(r => r.Html.WriteElement(tag, content, attributePairs));
            return this;
        }

        public ITemplateDefinition AddSelfClosingElement(PageArea pageArea, string tag, params string[] attributePairs)
        {
            ActionList(pageArea).Add(r => r.Html.WriteOpenTag(tag, true, attributePairs));
            return this;
        }

        public ITemplateDefinition AddElementOpen(PageArea pageArea, string tag, params string[] attributePairs)
        {
            WriteElementOpenTag(pageArea, tag, attributePairs);
            return this;
        }

        public ITemplateDefinition WriteScriptOpen(PageArea pageArea, string scriptType)
        {
            if (pageArea == PageArea.Initialization)
            {
                InitializationActions.Add(r => r.EnsureInitializationArea());
            }
            else
            {
                ActionList(pageArea).Add(r => r.Html.WriteScriptOpen(scriptType));
            }
            return this;
        }

        public ITemplateDefinition WriteScriptClose(PageArea pageArea)
        {
            if (pageArea != PageArea.Initialization)
                ActionList(pageArea).Add(r => r.Html.WriteScriptClose());
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

        public ITemplateDefinition AddElementClose(PageArea pageArea)
        {
            WriteElementCloseTag(pageArea);
            return this;
        }

        public ITemplateDefinition AddLineBreak(PageArea pageArea)
        {
            ActionList(pageArea).Add(r => r.Html.WriteLine());
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

        public ITemplateDefinition RepeatStart<T>(PageArea pageArea, string scopeName, string listScopeName)
        {
            return RepeatStart(pageArea, typeof(T), scopeName, listScopeName);
        }

        public ITemplateDefinition RepeatStart<T>(
            PageArea pageArea,
            Expression<Func<T, object>> propertyExpression,
            ITruthyEvaluator truthyEvaluator, 
            string scopeName, 
            string listScopeName)
        {
            var expression = (MemberExpression)propertyExpression.Body;
            var property = (PropertyInfo)expression.Member;

            return RepeatStart(pageArea, typeof(T), property, truthyEvaluator, scopeName, listScopeName);
        }

        public ITemplateDefinition AddData(PageArea pageArea, Type dataType, string scopeName = null)
        {
            // TODO: Figure out how to do this
            ActionList(pageArea).Add(r =>
                {
                    // r.Data.Set(dataType, value, scopeName);
                });
            return this;
        }

        public ITemplateDefinition AddData<T>(PageArea pageArea, string scopeName = null)
        {
            // TODO: Figure out how to do this
            ActionList(pageArea).Add(r =>
            {
                // r.Data.Set(typeof(T), value, scopeName);
            });
            return this;
        }

        public ITemplateDefinition ExtractProperty(PageArea pageArea, Type dataType, string propertyName, string scopeName = null, string propertyScopeName = null)
        {
            var property = dataType.GetProperties().FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));
            if (property == null)
                throw new TemplateBuilderException("Type " + dataType.DisplayName() + " does not have a public '" + propertyName + "' property");

            return ExtractProperty(pageArea, dataType, property, scopeName, propertyScopeName);
        }

        public ITemplateDefinition ExtractProperty<T>(PageArea pageArea, Expression<Func<T, object>> propertyExpression, string scopeName = null, string propertyScopeName = null)
        {
            var expression = (MemberExpression)propertyExpression.Body;
            var property = (PropertyInfo)expression.Member;

            return ExtractProperty(pageArea, typeof(T), property, scopeName, propertyScopeName);
        }

        private ITemplateDefinition ExtractProperty(PageArea pageArea, Type dataType, PropertyInfo property, string scopeName = null, string propertyScopeName = null)
        {
            ActionList(pageArea).Add(r =>
            {
                var obj = r.Data.Get(dataType, propertyScopeName);
                var value = property.GetValue(obj, null);
                r.Data.Set(property.PropertyType, value, scopeName);
            });
            return this;
        }

        public ITemplateDefinition RepeatStart(PageArea pageArea, Type dataTypeToRepeat, string scopeName, string listScopeName)
        {
            var repeat = new RepeatList(pageArea, dataTypeToRepeat, listScopeName, scopeName);
            ActionList(pageArea).Add(repeat.Enact);

            AddDependency(repeat.ListType, scopeName);

            if (_repeat != null)
                _repeatStack.Push(_repeat);

            _repeat = repeat;

            return this;
        }

        public ITemplateDefinition RepeatStart(
            PageArea pageArea,
            Type dataTypeToRepeat, 
            string propertyName, 
            ITruthyEvaluator truthyEvaluator, 
            string scopeName = null, 
            string listScopeName = null)
        {
            var property = dataTypeToRepeat.GetProperties().FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));
            if (property == null)
                throw new TemplateBuilderException("Type " + dataTypeToRepeat.DisplayName() + " does not have a public '" + propertyName + "' property");

            return RepeatStart(pageArea, dataTypeToRepeat, property, truthyEvaluator, scopeName, listScopeName);
        }

        private ITemplateDefinition RepeatStart(
            PageArea pageArea,
            Type dataTypeToRepeat, 
            PropertyInfo property, 
            ITruthyEvaluator truthyEvaluator, 
            string scopeName = null, 
            string listScopeName = null)
        {
            var repeat = new RepeatOnce(pageArea, dataTypeToRepeat, property, truthyEvaluator, listScopeName, scopeName);
            ActionList(pageArea).Add(repeat.Enact);

            AddDependency(repeat.RepeatType, scopeName);

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

        public ITemplateDefinition AddDataField<T>(PageArea pageArea, Expression<Func<T, object>> propertyExpression, IDataFieldFormatter dataFormatter = null, string scopeName = null)
        {
            var expression = (MemberExpression)propertyExpression.Body;
            var property = (PropertyInfo)expression.Member;

            return AddDataField(pageArea, typeof(T), property, dataFormatter, scopeName);
        }

        public ITemplateDefinition AddDataField<T>(PageArea pageArea, Func<T, string> formatFunc, string scopeName = null)
        {
            ActionList(pageArea).Add(r =>
            {
                var data = r.Data.Get<T>(scopeName);
                var formattedValue = formatFunc(data);
                r.Html.WriteText(formattedValue);
            });

            AddDependency(typeof(T), scopeName);

            return this;
        }

        public ITemplateDefinition AddDataField(PageArea pageArea, Type dataType, string propertyName, IDataFieldFormatter dataFormatter = null, string scopeName = null)
        {
            var property = dataType.GetProperties().FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));
            if (property == null)
                throw new TemplateBuilderException("Type " + dataType.DisplayName() + " does not have a public '" + propertyName + "' property");

            return AddDataField(pageArea, dataType, property, dataFormatter, scopeName);
        }

        private ITemplateDefinition AddDataField(PageArea pageArea, Type dataType, PropertyInfo property, IDataFieldFormatter dataFormatter = null, string scopeName = null)
        {
            ActionList(pageArea).Add(r =>
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

        public ITemplateDefinition AddText(PageArea pageArea, string assetName, string defaultText, bool isPreFormatted)
        {
            ActionList(pageArea).Add(r =>
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

        public ITemplateDefinition AddStaticJavascript(string rawJavascript)
        {
            var lines = rawJavascript
                .Split('\n')
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToArray();

            StaticJavascriptActions.Add(w =>
            {
                foreach(var line in lines)
                    w.WriteLineRaw(line);
            });
            return this;
        }

        public ITemplateDefinition AddStaticCss(string css)
        {
            var lines = css
                .Split('\n')
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToArray();

            StaticCssActions.Add(w =>
            {
                foreach(var line in lines)
                    w.WriteLineRaw(line);
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

        private RenderActionList ActionList(PageArea pageArea)
        {
            switch (pageArea)
            {
                case PageArea.Head: return HeadActions;
                case PageArea.Scripts: return ScriptActions;
                case PageArea.Styles: return StyleActions;
                case PageArea.Initialization: return InitializationActions;
                default: return BodyActions;
            }
        }

        private void WriteElementOpenTag(PageArea pageArea, string tag, params string[] attributePairs)
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
            ActionList(pageArea).Add(r => r.Html.WriteOpenTag(element.Tag, element.Attributes.ToArray()));
        }

        private void WriteElementCloseTag(PageArea pageArea)
        {
            var element = _element;
            ActionList(pageArea).Add(r => r.Html.WriteCloseTag(element.Tag));

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
