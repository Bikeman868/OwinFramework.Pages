using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Templates;

namespace OwinFramework.Pages.Html.Templates
{
    /// <summary>
    /// This is a template parser that inserts the contents
    /// of the template into the Javascript module asset of the
    /// element that renderes it
    /// </summary>
    public class JavascriptParser: ITemplateParser
    {
        private readonly ITemplateBuilder _templateBuilder;

        public JavascriptParser(
            ITemplateBuilder templateBuilder)
        {
            _templateBuilder = templateBuilder;
        }

        public ITemplate Parse(TemplateResource[] resources, IPackage package)
        {
            var template = _templateBuilder.BuildUpTemplate().PartOf(package);
            foreach (var resource in resources) 
            {
                var encoding = resource.Encoding ?? Encoding.UTF8;
                var javascript = encoding.GetString(resource.Content);
                template.AddStaticJavascript(javascript);
            }
            return template.Build();
        }
    }
}
