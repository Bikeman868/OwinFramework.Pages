using System.Text;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Templates;

namespace OwinFramework.Pages.Html.Templates
{
    /// <summary>
    /// This is a template parser that creates multi-part templates
    /// for components that have javascript, css and html in different
    /// files.
    /// It will:
    /// - Compile .less into .css
    /// - Include css and less in the styles part of the page
    /// - Include javascript in the script part of the page
    /// - Allow javascript strings to contain html by enclosing the html in back ticks
    /// - Supports mustache syntax in html files
    /// </summary>
    public class ComponentParser: ITemplateParser
    {
        private readonly ITemplateBuilder _templateBuilder;
        private readonly MustacheMixIn _mustacheMixIn;
        private readonly JavascriptMixIn _javascriptMixIn;
        private readonly CssMixIn _cssMixin;

        public ComponentParser(
            ITemplateBuilder templateBuilder)
        {
            _templateBuilder = templateBuilder;
            _mustacheMixIn = new MustacheMixIn();
            _javascriptMixIn = new JavascriptMixIn();
            _cssMixin = new CssMixIn();

            RenderIntoPage = true;
        }

        public bool RenderIntoPage { get; set; }

        public ITemplate Parse(TemplateResource[] resources, IPackage package)
        {
            var template = _templateBuilder.BuildUpTemplate().PartOf(package);
            foreach (var resource in resources) 
            {
                var encoding = resource.Encoding ?? Encoding.UTF8;
                var content = encoding.GetString(resource.Content);

                switch (resource.ContentType?.ToLower())
                {
                    case "application/javascript":
                        _javascriptMixIn.AddToTemplate(template, content, RenderIntoPage);
                        break;

                    case "text/css":
                        _cssMixin.AddCssToTemplate(template, content, RenderIntoPage);
                        break;

                    case "text/less":
                        _cssMixin.AddLessToTemplate(template, content, RenderIntoPage, true);
                        break;

                    case "text/html":
                        _mustacheMixIn.AddToTemplate(template, content);
                        break;

                    default:
                        template.AddHtml(content);
                        break;
                }
            }
            return template.Build();
        }
    }
}
