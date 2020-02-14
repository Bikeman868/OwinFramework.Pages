using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dotless.Core.configuration;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Templates;

namespace OwinFramework.Pages.Html.Templates
{
    /// <summary>
    /// This is a template parser that inserts the contents
    /// of the template into the CSS module asset of the
    /// element that renderes it or directly into the page
    /// </summary>
    public class CssParser: ITemplateParser
    {
        private readonly ITemplateBuilder _templateBuilder;

        public CssParser(
            ITemplateBuilder templateBuilder)
        {
            _templateBuilder = templateBuilder;
            RenderIntoPage = true;
        }

        public bool RenderIntoPage { get; set; }

        public ITemplate Parse(TemplateResource[] resources, IPackage package)
        {
            var template = _templateBuilder.BuildUpTemplate().PartOf(package);
            foreach (var resource in resources) 
            {
                var encoding = resource.Encoding ?? Encoding.UTF8;
                var css = encoding.GetString(resource.Content);

                if (resource.ContentType == "text/less")
                {
                    var dotlessConfig = new DotlessConfiguration { MinifyOutput = true };
                    css = dotless.Core.Less.Parse(css, dotlessConfig);
                }

                if (RenderIntoPage)
                {
                    foreach(var line in css.Split('\n'))
                        template.AddStyleLine(line);
                }
                else
                {
                    template.AddStaticCss(css);
                }
            }
            return template.Build();
        }
    }
}
