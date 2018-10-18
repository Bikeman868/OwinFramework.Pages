using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Elements
{
    /// <summary>
    /// This is a derivative of Component that renders a template.
    /// </summary>
    public class TemplateComponent : Component
    {
        private string _templatePath;

        public TemplateComponent(IComponentDependenciesFactory dependencies)
            : base(dependencies)
        {
        }

        public override IEnumerable<PageArea> GetPageAreas()
        {
            return new[] { PageArea.Body };
        }

        protected override string GetCommentName()
        {
            return "template " + _templatePath;
        }

        /// <summary>
        /// Specifies the path to the template to render
        /// </summary>
        public void Template(string templatePath)
        {
            _templatePath = templatePath;
            HtmlWriters = new Action<IRenderContext>[] { RenderTemplate };
        }

        private void RenderTemplate(IRenderContext renderContext)
        {
            var template = Dependencies.TemplateManager.Get(renderContext, _templatePath);
            template.WritePageArea(renderContext, PageArea.Body);
        }
    }
}
