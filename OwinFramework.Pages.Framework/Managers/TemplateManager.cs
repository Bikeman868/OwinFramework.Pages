using System;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Framework.Managers
{
    internal class TemplateManager : ITemplateManager
    {
        private ITemplate _dummyTemplate;

        public TemplateManager()
        {
        }

        ITemplateManager ITemplateManager.Register(ITemplate template, string templatePath, params string[] locales)
        {
            _dummyTemplate = template;
            return this;
        }

        ITemplate ITemplateManager.Get(IRenderContext renderContext, string templatePath)
        {
            return _dummyTemplate;
        }

    }
}
