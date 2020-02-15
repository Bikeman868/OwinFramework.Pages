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

        private bool _renderCssIntoPage;
        private bool _renderJavascriptIntoPage;
        private bool _minifyCss;

        public ComponentParser(
            ITemplateBuilder templateBuilder)
        {
            _templateBuilder = templateBuilder;
            _mustacheMixIn = new MustacheMixIn();
            _javascriptMixIn = new JavascriptMixIn();
            _cssMixin = new CssMixIn();
        }

        public ComponentParser RenderCssIntoPage(bool renderCssIntoPage = true)
        {
            _renderCssIntoPage = renderCssIntoPage;
            return this;
        }

        public ComponentParser RenderJavascriptIntoPage(bool renderJavascriptIntoPage = true)
        {
            _renderJavascriptIntoPage = renderJavascriptIntoPage;
            return this;
        }

        public ComponentParser MinifyCss(bool minifyCss = true)
        {
            _minifyCss = minifyCss;
            return this;
        }

        public ITemplate Parse(TemplateResource[] resources, IPackage package, IModule module)
        {
            var template = _templateBuilder
                .BuildUpTemplate()
                .PartOf(package)
                .DeployIn(module);

            foreach (var resource in resources) 
            {
                var encoding = resource.Encoding ?? Encoding.UTF8;
                var content = encoding.GetString(resource.Content);

                switch (resource.ContentType?.ToLower())
                {
                    case "application/javascript":
                        _javascriptMixIn.AddToTemplate(template, content, _renderJavascriptIntoPage);
                        break;

                    case "text/css":
                        _cssMixin.AddCssToTemplate(template, content, _renderCssIntoPage);
                        break;

                    case "text/less":
                        _cssMixin.AddLessToTemplate(template, content, _renderCssIntoPage, _minifyCss);
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
