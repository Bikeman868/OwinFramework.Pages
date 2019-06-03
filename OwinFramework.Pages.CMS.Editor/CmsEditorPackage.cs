using System.Collections.Generic;
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
    /// <summary>
    /// This package exports a layout and a region called "cms:editor" that
    /// you can refer to in your application to include the CMS editor into
    /// a page on your website.
    /// This package optionally adds a page to your website containing the
    /// CMS editor. You can enable this page by setting a URL path in the
    /// Urchin configuration.
    /// </summary>
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
                .Name("liveUpdate")
                .Route(_configuration.ServiceBasePath + "live-update/", new []{ Method.Get, Method.Post, Method.Delete }, 0)
                .RequiredPermission(Permissions.View, false)
                .CreateComponent("liveUpdateClient")
                .Build();

            // Load templates and accumulate all of the CSS and JS assets

            var script = new StringBuilder();
            var less = new StringBuilder();

            var liveUpdateLogTemplate = AddVueTemplate(script, less, "LiveUpdateLog");

            // Load JavaScript modules and concatenate all the JavaScript

            var scriptModules = new List<string>();

            LoadScriptModule("liveUpdateModule", scriptModules);

            // Output JavaScript and CSS assets in a module asset

            var module = fluentBuilder.BuildUpModule()
                .Name("cms_editor")
                .AssetDeployment(AssetDeployment.PerModule)
                .Build();

            var assetsComponentBuilder = fluentBuilder.BuildUpComponent(null)
                .Name("assets")
                .DeployIn(module)
                .DeployFunction(null, "initVue", null, script.ToString(), true)
                .RenderInitialization("cms-editor-init", "<script>ns." + NamespaceName + ".initVue();</script>")
                .DeployLess(less.ToString());

            foreach (var scriptModule in scriptModules)
                assetsComponentBuilder.DeployScript(scriptModule);

            var assetsComponent = assetsComponentBuilder.Build();

            // Define the elements that applications can reference to
            // include the CMS editor into a page on their website

            var editorRegion = fluentBuilder.BuildUpRegion()
                .Name("editor")
                .AssetDeployment(AssetDeployment.PerModule)
                .DeployIn(module)
                .NeedsComponent("libraries:vue")
                .NeedsComponent("ajax:ajax")
                .NeedsComponent("liveUpdateClient")
                .NeedsComponent(assetsComponent)
                .AddTemplate(liveUpdateLogTemplate)
                .Build();

            var editorLayout = fluentBuilder.BuildUpLayout()
                .Name("editor")
                .ZoneNesting("main")
                .Region("main", editorRegion)
                .Build();

            if (!string.IsNullOrEmpty(_configuration.EditorPath))
            {
                fluentBuilder.BuildUpPage()
                    .Name("editor")
                    .Route(_configuration.EditorPath, 0, Method.Get)
                    .Layout(editorLayout)
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

            var markupLines = GetEmbeddedTextFile(markupFileName);
            if (markupLines != null)
            {
                foreach (var line in GetEmbeddedTextFile(markupFileName))
                {
                    templateDefinition.AddHtml(line);
                    templateDefinition.AddLineBreak();
                }
            }

            var viewModel = GetEmbeddedTextFile(viewModelFileName);
            if (viewModel != null)
            {
                foreach (var line in viewModel)
                    script.AppendLine(line);
            }

            var styles = GetEmbeddedTextFile(stylesheetFileName);
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

        private void LoadScriptModule(string moduleName, List<string> modules)
        {
            var lines = GetEmbeddedTextFile(moduleName + ".js");
            if (lines != null)
                modules.Add(string.Join("\n", lines));
        }

        private string[] GetEmbeddedTextFile(string templateName)
        {
            var resource = _resourceManager.GetResource(Assembly.GetExecutingAssembly(), templateName);
            if (resource.Content == null) return null;

            var text = Encoding.UTF8.GetString(resource.Content);
            return text.Replace("\r", "").Split('\n');
        }
    }
}
