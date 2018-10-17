using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Core.Interfaces.Templates;

namespace OwinFramework.Pages.Html.Templates
{
    internal class TemplateDefinition : ITemplateDefinition
    {
        private readonly INameManager _nameManager;
        private readonly ITemplateManager _templateManager;
        private readonly Template _template;
        private readonly List<Action<IRenderContext>> _renderActions;

        private IPackage _package;
        private Stack<Element> _elementStack;
        private Element _element;

        private class Element
        {
            public string Tag;
            public List<string> Attributes = new List<string>();
        }

        public TemplateDefinition(
            INameManager nameManager,
            ITemplateManager templateManager)
        {
            _nameManager = nameManager;
            _templateManager = templateManager;

            _renderActions = new List<Action<IRenderContext>>();
            _elementStack = new Stack<Element>();
            _template = new Template();
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
                    "here since there is no current element");

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
            return this;
        }

        public ITemplateDefinition AddElementClose()
        {
            WriteElementCloseTag();
            return this;
        }

        public ITemplateDefinition AddLayout(string layoutName)
        {
            return this;
        }

        public ITemplateDefinition AddLayout(ILayout layout)
        {
            return this;
        }

        public ITemplateDefinition AddRegion(string regionName)
        {
            return this;
        }

        public ITemplateDefinition AddRegion(IRegion region)
        {
            return this;
        }

        public ITemplateDefinition AddComponent(string componentName)
        {
            return this;
        }

        public ITemplateDefinition AddComponent(IComponent component)
        {
            return this;
        }

        public ITemplateDefinition AddTemplate(string templatePath)
        {
            return this;
        }

        public ITemplateDefinition AddTemplate(ITemplate template)
        {
            return this;
        }

        public ITemplateDefinition RepeatStart(Type dataTypeToRepeat)
        {
            return this;
        }

        public ITemplateDefinition RepeatEnd()
        {
            return this;
        }

        public ITemplateDefinition AddDataField(Type dataType, string propertyName, IDataFieldFormatter dataFormatter = null, string scopeName = null)
        {
            return this;
        }

        public ITemplateDefinition AddText(string assetName, string defaultText)
        {
            return this;
        }

        public ITemplate Build()
        {
            _template.Add(_renderActions);
            return _template;
        }

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

            _renderActions.Add(r =>
                {
                    r.Html.WriteOpenTag(_element.Tag, _element.Attributes.ToArray());
                    r.Html.WriteLine();
                });
        }

        private void WriteElementCloseTag()
        {
            _renderActions.Add(r => r.Html.WriteCloseTag(_element.Tag));

            if (_elementStack.Count > 0)
                _element = _elementStack.Pop();
            else
                _element = null;
        }
    }
}
