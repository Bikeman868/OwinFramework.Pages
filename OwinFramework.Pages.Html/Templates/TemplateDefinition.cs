using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly List<Action<IRenderContext>> _renderActions;
        private readonly Stack<Element> _elementStack;

        private IPackage _package;
        private Element _element;

        private class Element
        {
            public string Tag;
            public List<string> Attributes = new List<string>();
        }

        public TemplateDefinition(
            INameManager nameManager,
            IAssetManager assetManager,
            IDataConsumerFactory dataConsumerFactory)
        {
            _nameManager = nameManager;
            _assetManager = assetManager;

            _renderActions = new List<Action<IRenderContext>>();
            _elementStack = new Stack<Element>();
            _template = new Template(dataConsumerFactory);
        }

        public ITemplateDefinition PartOf(string packageName)
        {
            _package = _nameManager.ResolvePackage(packageName);
            ((ITemplate)_template).Package = _package;
            return this;
        }

        public ITemplateDefinition PartOf(IPackage package)
        {
            _package = package;
            ((ITemplate)_template).Package = package;
            return this;
        }

        public ITemplateDefinition AddHtml(string html)
        {
            _renderActions.Add(r => r.Html.Write(html));
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

            _renderActions.Add(_renderActions[_renderActions.Count - 1]);
            _renderActions[_renderActions.Count - 2] = r =>
                {
                    var data = r.Data.Get(dataType, scopeName);
                    var propertyValue = property.GetValue(data, null);
                    var formattedValue = dataFormatter == null ? propertyValue.ToString() : dataFormatter.Format(property, propertyValue);
                    element.Attributes[attributeIndex] = formattedValue;
                };

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

        public ITemplateDefinition AddLayout(string layoutName)
        {
            var layout = _nameManager.ResolveLayout(layoutName, _package);

            if (layout == null)
            {
                var index = _renderActions.Count;
                _renderActions.Add(null);
                _nameManager.AddResolutionHandler(NameResolutionPhase.ResolveElementReferences, nm =>
                {
                    var e = nm.ResolveLayout(layoutName, _package);
                    _renderActions[index] = r => e.WritePageArea(r, PageArea.Body, (rc, pa, rn) => WriteResult.Continue()); // TODO: Write layout regions
                    AddConsumerNeeds(e as IDataConsumer);
                });
            }
            else
            {
                AddLayout(layout);
            }

            return this;
        }

        public ITemplateDefinition AddLayout(ILayout layout)
        {
            _renderActions.Add(r => layout.WritePageArea(r, PageArea.Body, (rc, pa, rn) => WriteResult.Continue())); // TODO: Write layout regions
            AddConsumerNeeds(layout as IDataConsumer);

            return this;
        }

        public ITemplateDefinition AddRegion(string regionName)
        {
            var region = _nameManager.ResolveRegion(regionName, _package);

            if (region == null)
            {
                var index = _renderActions.Count;
                _renderActions.Add(null);
                _nameManager.AddResolutionHandler(NameResolutionPhase.ResolveElementReferences, nm =>
                {
                    var e = nm.ResolveRegion(regionName, _package);
                    _renderActions[index] = r => e.WritePageArea(r, PageArea.Body, null, (rc, pa) => WriteResult.Continue()); // TODO: Write region
                    AddConsumerNeeds(e as IDataConsumer);
                });
            }
            else
            {
                AddRegion(region);
            }

            return this;
        }

        public ITemplateDefinition AddRegion(IRegion region)
        {
            _renderActions.Add(r => region.WritePageArea(r, PageArea.Body, null, (rc, pa) => WriteResult.Continue())); // TODO: Write region
            AddConsumerNeeds(region as IDataConsumer);

            return this;
        }

        public ITemplateDefinition AddComponent(string componentName)
        {
            var component = _nameManager.ResolveComponent(componentName, _package);

            if (component == null)
            {
                var index = _renderActions.Count;
                _renderActions.Add(null);
                _nameManager.AddResolutionHandler(NameResolutionPhase.ResolveElementReferences, nm =>
                    {
                        var e = nm.ResolveComponent(componentName, _package);
                        _renderActions[index] = r => e.WritePageArea(r, PageArea.Body);
                        AddConsumerNeeds(e as IDataConsumer);
                    });
            }
            else
            {
                AddComponent(component);
            }

            return this;
        }

        public ITemplateDefinition AddComponent(IComponent component)
        {
            _renderActions.Add(r => component.WritePageArea(r, PageArea.Body));
            AddConsumerNeeds(component as IDataConsumer);

            return this;
        }

        public ITemplateDefinition AddTemplate(string templatePath)
        {
            var template = _nameManager.ResolveTemplate(templatePath);

            if (template == null)
            {
                var index = _renderActions.Count;
                _renderActions.Add(null);

                _nameManager.AddResolutionHandler(NameResolutionPhase.ResolveElementReferences, nm =>
                    {
                        var t = nm.ResolveTemplate(templatePath);
                        if (t == null)
                        {
                            _renderActions[index] = r => r.Html.WriteElementLine("p", "Unknown template " + templatePath);
                        }
                        else
                        {
                            _renderActions[index] = r => t.WritePageArea(r, PageArea.Body);
                            AddConsumerNeeds(t as IDataConsumer);
                        }
                    });
            }
            else
            {
                AddTemplate(template);
            }

            return this;
        }

        public ITemplateDefinition AddTemplate(ITemplate template)
        {
            _renderActions.Add(r => template.WritePageArea(r, PageArea.Body));
            AddConsumerNeeds(template as IDataConsumer);

            return this;
        }

        public ITemplateDefinition RepeatStart(Type dataTypeToRepeat, string scopeName, string listScopeName)
        {
            return this;
        }

        public ITemplateDefinition RepeatEnd()
        {
            return this;
        }

        public ITemplateDefinition AddDataField(Type dataType, string propertyName, IDataFieldFormatter dataFormatter = null, string scopeName = null)
        {
            var property = dataType.GetProperties().FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));
            if (property == null)
                throw new TemplateBuilderException("Type " + dataType.DisplayName() + " does not have a public '" + propertyName + "' property");

            _renderActions.Add( r =>
            {
                var data = r.Data.Get(dataType, scopeName);
                var propertyValue = property.GetValue(data, null);
                var formattedValue = dataFormatter == null ? propertyValue.ToString() : dataFormatter.Format(property, propertyValue);
                r.Html.Write(formattedValue);
            });

            var dataConsumer = _template as IDataConsumer;
            if (dataConsumer != null)
                dataConsumer.HasDependency(dataType, scopeName);

            return this;
        }

        public ITemplateDefinition AddText(string assetName, string defaultText)
        {
            _renderActions.Add(r =>
            {
                var localizedString = _assetManager.GetLocalizedText(r , assetName, defaultText);
                r.Html.Write(localizedString);
            });
            return this;
        }

        public ITemplate Build()
        {
            _template.Add(_renderActions);
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
            _renderActions.Add(r =>
                {
                    r.Html.WriteOpenTag(element.Tag, element.Attributes.ToArray());
                    r.Html.WriteLine();
                });
        }

        private void WriteElementCloseTag()
        {
            var element = _element;
            _renderActions.Add(r => r.Html.WriteCloseTag(element.Tag));

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

        #endregion

    }
}
