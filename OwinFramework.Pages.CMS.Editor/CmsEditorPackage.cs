using OwinFramework.Pages.Core.Interfaces;
using System.Linq;
using System.Reflection;
using System.Text;
using OwinFramework.Pages.CMS.Editor.Configuration;
using OwinFramework.Pages.CMS.Editor.Services;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Core.Interfaces.Templates;
using Urchin.Client.Interfaces;
using OwinFramework.Interfaces.Utility;
using OwinFramework.MiddlewareHelpers.EmbeddedResources;

namespace OwinFramework.Pages.CMS.Editor
{
    public class CmsEditorPackage: IPackage
    {
        private readonly ITemplateBuilder _templateBuilder;
        private readonly INameManager _nameManager;
        private readonly EditorConfiguration _configuration;
        private readonly ResourceManager _resourceManager;

        public string NamespaceName { get; set; }
        public IModule Module { get; set; }
        public ElementType ElementType { get { return ElementType.Package; } }
        public string Name { get; set; }

        public CmsEditorPackage(
            IHostingEnvironment hostingEnvironment,
            ITemplateBuilder templateBuilder,
            INameManager nameManager,
            IConfigurationStore configurationStore)
        {
            _templateBuilder = templateBuilder;
            _nameManager = nameManager;

            Name = "cms_editor";
            _resourceManager = new ResourceManager(hostingEnvironment, new MimeTypeEvaluator());
            _configuration = new EditorConfiguration(configurationStore);
        }

        IPackage IPackage.Build(IFluentBuilder fluentBuilder)
        {
            // Build the API services

            fluentBuilder.BuildUpService(null, typeof(LiveUpdateService))
                .Route(_configuration.ServiceBasePath + "live-update/", new []{ Method.Get }, 0)
                .RequiredPermission(Permissions.View, false)
                .Build();

            // Load templates and accumulate all of the CSS and JS assets

            var script = new StringBuilder();
            var less = new StringBuilder();

            var liveUpdateLogTemplate = AddVueTemplate(script, less, "LiveUpdateLog");

            // JavaScript and CSS assets

            var module = fluentBuilder.BuildUpModule()
                .Name("cms_editor")
                .AssetDeployment(AssetDeployment.PerModule)
                .Build();

            var assetsComponent = fluentBuilder.BuildUpComponent(null)
                .Name("assets")
                .DeployIn(module)
                .DeployFunction(null, "init", null, script.ToString(), true)
                .DeployLess(less.ToString())
                .RenderInitialization("cms-editor-init", "<script>ns." + NamespaceName + ".init();</script>")
                .Build();

            // Define internal elements

            // ...

            // Define the top level elements

            var editorRegion = fluentBuilder.BuildUpRegion()
                .Name("editor")
                .AssetDeployment(AssetDeployment.PerModule)
                .DeployIn(module)
                .NeedsComponent("libraries:vue")
                .NeedsComponent(assetsComponent)
                .AddTemplate(liveUpdateLogTemplate)
                .Build();

            var pageLayout = fluentBuilder.BuildUpLayout()
                .Name("editor")
                .ZoneNesting("main")
                .Region("main", editorRegion)
                .Build();

            if (!string.IsNullOrEmpty(_configuration.EditorPath))
            {
                fluentBuilder.BuildUpPage()
                    .Name("editor")
                    .Route(_configuration.EditorPath, 0, Method.Get)
                    .Layout(pageLayout)
                    .Build();
            }

            return this;
        }

        private string AddVueTemplate(
            StringBuilder script, 
            StringBuilder less,
            string baseName)
        {
            var templateDefinition = _templateBuilder.BuildUpTemplate();

            var markupFileName = baseName + ".html";
            var viewModelFileName = baseName + ".js";
            var stylesheetFileName = baseName + ".less";

            var markupLines = GetEmbeddedTemplate(markupFileName);
            if (markupLines != null)
            {
                foreach (var line in GetEmbeddedTemplate(markupFileName))
                {
                    templateDefinition.AddHtml(line);
                    templateDefinition.AddLineBreak();
                }
            }

            var viewModel = GetEmbeddedTemplate(viewModelFileName);
            if (viewModel != null)
            {
#if DEBUG
                foreach (var line in viewModel)
                    templateDefinition.AddInitializationLine(line);
#else
                foreach (var line in viewModel)
                    script.AppendLine(line);
#endif
            }

            var styles = GetEmbeddedTemplate(stylesheetFileName);
            if (styles != null)
            {
                foreach (var line in styles)
                    less.AppendLine(line);
            }

            var template = templateDefinition.Build();
            var templatePath = _configuration.TemplateBasePath + baseName;

            _nameManager.Register(template, templatePath);

            return templatePath;
        }

        private string[] GetEmbeddedTemplate(string templateName)
        {
            var resource = _resourceManager.GetResource(Assembly.GetExecutingAssembly(), templateName);
            if (resource.Content == null) return null;

            var text = Encoding.UTF8.GetString(resource.Content);
            return text.Replace("\r", "").Split('\n');
        }
    }
}
