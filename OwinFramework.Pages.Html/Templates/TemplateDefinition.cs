using System;
using System.Collections.Generic;
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
        private readonly List<Func<IRenderContext, IWriteResult>> _elements;

        private IPackage _package;

        public TemplateDefinition(
            INameManager nameManager,
            ITemplateManager templateManager)
        {
            _nameManager = nameManager;
            _templateManager = templateManager;

            _elements = new List<Func<IRenderContext, IWriteResult>>();
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
            throw new NotImplementedException();
        }

        public ITemplateDefinition AddElementOpen(string tag, params string[] attributePairs)
        {
            throw new NotImplementedException();
        }

        public ITemplateDefinition SetElementAttribute(string attributeName, Type dataType, string propertyName, IDataFieldFormatter dataFormatter = null, string scopeName = null)
        {
            throw new NotImplementedException();
        }

        public ITemplateDefinition AddElementClose()
        {
            throw new NotImplementedException();
        }

        public ITemplateDefinition AddLayout(string layoutName)
        {
            throw new NotImplementedException();
        }

        public ITemplateDefinition AddLayout(ILayout layout)
        {
            throw new NotImplementedException();
        }

        public ITemplateDefinition AddRegion(string regionName)
        {
            throw new NotImplementedException();
        }

        public ITemplateDefinition AddRegion(IRegion region)
        {
            throw new NotImplementedException();
        }

        public ITemplateDefinition AddComponent(string componentName)
        {
            throw new NotImplementedException();
        }

        public ITemplateDefinition AddComponent(IComponent component)
        {
            throw new NotImplementedException();
        }

        public ITemplateDefinition AddTemplate(string templatePath)
        {
            throw new NotImplementedException();
        }

        public ITemplateDefinition AddTemplate(ITemplate template)
        {
            throw new NotImplementedException();
        }

        public ITemplateDefinition RepeatStart(Type dataTypeToRepeat)
        {
            throw new NotImplementedException();
        }

        public ITemplateDefinition RepeatEnd()
        {
            throw new NotImplementedException();
        }

        public ITemplateDefinition AddDataField(Type dataType, string propertyName, IDataFieldFormatter dataFormatter = null, string scopeName = null)
        {
            throw new NotImplementedException();
        }

        public ITemplateDefinition AddText(string assetName, string defaultText)
        {
            throw new NotImplementedException();
        }

        public ITemplate Build()
        {
            _template.Add(_elements);
            return _template;
        }
    }
}
