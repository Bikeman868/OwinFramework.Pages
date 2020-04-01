﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Templates;

namespace OwinFramework.Pages.Html.Templates
{
    /// <summary>
    /// This is a template parser that inserts the contents
    /// of the template directly into the page without changing
    /// anything. This is good for snippets of static Html, embedding
    /// SVG directly into the page etc. This parser adds the content
    /// to the appropriate area of the page according to the content
    /// type of the resource.
    /// </summary>
    public class AsIsParser: ITemplateParser
    {
        private readonly ITemplateBuilder _templateBuilder;

        public AsIsParser(
            ITemplateBuilder templateBuilder)
        {
            _templateBuilder = templateBuilder;
        }

        public ITemplate Parse(TemplateResource[] resources, IPackage package, IModule module)
        {
            var template = _templateBuilder.BuildUpTemplate().PartOf(package).DeployIn(module);
            foreach (var resource in resources) 
            {
                var encoding = resource.Encoding ?? Encoding.UTF8;
                var html = encoding.GetString(resource.Content);
                switch (resource.ContentType?.ToLower())
                {
                    case "application/javascript":
                        foreach (var line in html.Split('\n'))
                            template.AddHtml(PageArea.Initialization, line);
                        break;

                    case "text/css":
                        foreach(var line in html.Split('\n'))
                            template.AddHtml(PageArea.Styles, line);
                        break;

                    case "text/less":
                        throw new NotImplementedException(
                            "You can not render .less directly into the page. Consider using the ComponentParser instead");

                    default:
                        template.AddHtml(PageArea.Body, html);
                        break;
                }
            }
            return template.Build();
        }
    }
}
