using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Templates;

namespace OwinFramework.Pages.Html.Templates
{
    internal class TemplateFactory : ITemplateFactory
    {
        private readonly INameManager _nameManager;
        private readonly ITemplateManager _templateManager;

        public TemplateFactory(
            INameManager nameManager,
            ITemplateManager templateManager)
        {
            _nameManager = nameManager;
            _templateManager = templateManager;
        }

        public ITemplateDefinition Create()
        {
            return new TemplateDefinition(
                _nameManager,
                _templateManager);
        }
    }
}
