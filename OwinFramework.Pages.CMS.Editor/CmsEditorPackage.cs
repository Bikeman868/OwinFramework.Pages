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
using OwinFramework.Pages.CMS.Editor.Assets;

namespace OwinFramework.Pages.CMS.Editor
{
    public class CmsEditorPackage: IPackage
    {
        private readonly ITemplateBuilder _templateBuilder;
        private readonly INameManager _nameManager;
        private readonly IJavascriptWriterFactory _javascriptWriterFactory;
        private readonly IServiceDependenciesFactory _serviceDependenciesFactory;
        private readonly IComponentDependenciesFactory _componentDependenciesFactory;
        private readonly EditorConfiguration _configuration;
        private readonly ResourceManager _resourceManager;

        string IPackage.NamespaceName { get; set; }
        IModule IPackage.Module { get; set; }
        ElementType INamed.ElementType { get { return ElementType.Package; } }
        string INamed.Name { get; set; }

        public CmsEditorPackage(
            IHostingEnvironment hostingEnvironment,
            ITemplateBuilder templateBuilder,
            INameManager nameManager,
            IConfigurationStore configurationStore,
            IJavascriptWriterFactory javascriptWriterFactory,
            IServiceDependenciesFactory serviceDependenciesFactory,
            IComponentDependenciesFactory componentDependenciesFactory)
        {
            _templateBuilder = templateBuilder;
            _nameManager = nameManager;
            _javascriptWriterFactory = javascriptWriterFactory;
            _serviceDependenciesFactory = serviceDependenciesFactory;
            _componentDependenciesFactory = componentDependenciesFactory;

            var mimeTypeEvaluator = new MimeTypeEvaluator();
            _resourceManager = new ResourceManager(hostingEnvironment, mimeTypeEvaluator);

            _configuration = new EditorConfiguration(configurationStore);
        }

        IPackage IPackage.Build(IFluentBuilder fluentBuilder)
        {
            // Build the API services

            fluentBuilder.BuildUpService(null, typeof(LiveUpdateService))
                .Route(_configuration.ServiceBasePath + "live-update/", new []{ Method.Get }, 0)
                .Build();

            // Load templates and generate CSS and JS assets

            var javascriptWriter = _javascriptWriterFactory.Create(HtmlFormat.Html, true, false);

            var liveUpdateLogTemplate = AddVueTemplate(javascriptWriter, "LiveUpdateLog");

            // Buils the assets service to serve the assets

            fluentBuilder.BuildUpService(
                new AssetsService(
                    _serviceDependenciesFactory,
                    javascriptWriter.ToLines(), 
                    null))
                .Route(_configuration.ServiceBasePath + "assets/", new []{ Method.Get }, 0)
                .Build();

            var assetsLinksComponent = fluentBuilder.BuildUpComponent(
                new AssetLinksComponent(_configuration, _componentDependenciesFactory))
                .Build();

            var module = fluentBuilder.BuildUpModule()
                .Name("cms")
                .AssetDeployment(AssetDeployment.PerModule)
                .Build();

            // Define internal elements

            // ...

            // Define the top level elements

            var editorRegion = fluentBuilder.BuildUpRegion()
                .Name("editor")
                .NeedsComponent("libraries:vue")
                .AssetDeployment(AssetDeployment.PerModule)
                .DeployIn(module)
                .NeedsComponent(assetsLinksComponent)
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

        private string AddVueTemplate(IJavascriptWriter javascriptWriter, string baseName)
        {
            var templateDefinition = _templateBuilder.BuildUpTemplate();

            var markupName = baseName + ".html";
            var viewModelName = baseName + ".js";
            var stylesheetName = baseName + ".css";

            var markupLines = GetEmbeddedTemplate(markupName);
            if (markupLines != null)
            {
                foreach (var line in GetEmbeddedTemplate(markupName))
                {
                    templateDefinition.AddHtml(line);
                    templateDefinition.AddLineBreak();
                }
            }

            var viewModel = GetEmbeddedTemplate(viewModelName);
            if (viewModel != null)
            {
                foreach (var line in viewModel)
                    javascriptWriter.WriteLineRaw(line, this);
            }

            var styles = GetEmbeddedTemplate(stylesheetName);
            if (styles != null)
            {
                foreach (var line in styles)
                    templateDefinition.AddStyleLine(line);
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
