using System.Text;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Runtime;
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
        private bool _indented;
        private AssetDeployment _assetDeployment;

        public ComponentParser(
            ITemplateBuilder templateBuilder, 
            IFrameworkConfiguration frameworkConfiguration)
        {
            _templateBuilder = templateBuilder;
            _mustacheMixIn = new MustacheMixIn();
            _javascriptMixIn = new JavascriptMixIn();
            _cssMixin = new CssMixIn();
            _assetDeployment = AssetDeployment.PerModule;

            frameworkConfiguration.Subscribe(fc =>
            {
                _minifyCss = fc.MinifyCss;
                _indented = fc.Indented;
            });
        }

        /// <summary>
        /// Forces CSS to be rendered into the page nomatter which module is
        /// assigned or which asset deployment scenario was selected. Passing
        /// false to this method reverts rendering location for CSS to be
        /// defined by the module and asset deployment scheme chosen.
        /// This method is useful only if you want the Javascript and CSS
        /// to be rendered in different places, otherwise you can just call
        /// the DeployAssets() method to redirect all assets.
        /// </summary>
        public ComponentParser RenderCssIntoPage(bool renderCssIntoPage = true)
        {
            _renderCssIntoPage = renderCssIntoPage;
            return this;
        }

        /// <summary>
        /// Forces Javascript to be rendered into the page nomatter which module is
        /// assigned or which asset deployment scenario was selected. Passing
        /// false to this method reverts rendering location for Javascript to be
        /// defined by the module and asset deployment scheme chosen.
        /// This method is useful only if you want the Javascript and CSS
        /// to be rendered in different places, otherwise you can just call
        /// the DeployAssets() method to redirect all assets.
        /// </summary>
        public ComponentParser RenderJavascriptIntoPage(bool renderJavascriptIntoPage = true)
        {
            _renderJavascriptIntoPage = renderJavascriptIntoPage;
            return this;
        }

        /// <summary>
        /// Specifies how CSS and Javascript should be deployed. The default is
        /// to deploy into a module if a module was defined for the template loader
        /// or global to the website otherwise. Setting this to inherit makes the
        /// template take on the deployment mechnism of the region that is rendering
        /// the template
        /// </summary>
        public ComponentParser DeployAssets(AssetDeployment assetDeployment)
        {
            _assetDeployment = assetDeployment;
            return this;
        }

        public ITemplate Parse(TemplateResource[] resources, IPackage package, IModule module)
        {
            var template = _templateBuilder.BuildUpTemplate();
            if (package != null) template.PartOf(package);

            if (_assetDeployment == AssetDeployment.PerModule)
            {
                if (module == null)
                    template.AssetDeployment(AssetDeployment.PerWebsite);
                else
                    template.DeployIn(module);
            }
            else
            {
                template.AssetDeployment(_assetDeployment);
            }

            foreach (var resource in resources) 
            {
                var encoding = resource.Encoding ?? Encoding.UTF8;
                var content = encoding.GetString(resource.Content);

                switch (resource.ContentType?.ToLower())
                {
                    case "application/javascript":
                        _javascriptMixIn.AddToTemplate(template, content, _renderJavascriptIntoPage, _indented);
                        break;

                    case "text/css":
                        _cssMixin.AddCssToTemplate(template, content, _renderCssIntoPage);
                        break;

                    case "text/less":
                        _cssMixin.AddLessToTemplate(template, content, _renderCssIntoPage, _minifyCss);
                        break;

                    case "text/html":
                        _mustacheMixIn.AddToTemplate(template, PageArea.Body, content);
                        break;

                    default:
                        template.AddHtml(PageArea.Body, content);
                        break;
                }
            }
            return template.Build();
        }
    }
}
