using System;
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
    /// This package exports a layout and a region called "cms:manager" that
    /// you can refer to in your application to include the CMS manager into
    /// a page on your website.
    /// This package optionally adds a page to your website containing the
    /// CMS manager. You can enable this page by setting a URL path in the
    /// Urchin configuration.
    /// </summary>
    public class CmsEditorPackage: IPackage
    {
        private readonly ITemplateBuilder _templateBuilder;
        private readonly INameManager _nameManager;
        private readonly ResourceManager _resourceManager;
        private readonly IDisposable _configNotification;

        public string NamespaceName { get; set; }
        public IModule Module { get; set; }
        public ElementType ElementType { get { return ElementType.Package; } }
        public string Name { get; set; }

        private EditorConfiguration _configuration;

        public CmsEditorPackage(
            IHostingEnvironment hostingEnvironment,
            ITemplateBuilder templateBuilder,
            INameManager nameManager,
            IConfigurationStore configurationStore)
        {
            _templateBuilder = templateBuilder;
            _nameManager = nameManager;

            Name = "cms_editor";
            NamespaceName = "cmseditor";
            _resourceManager = new ResourceManager(hostingEnvironment, new MimeTypeEvaluator());

            _configNotification = configurationStore.Register(
                EditorConfiguration.Path, 
                c => _configuration = c.Sanitize(), 
                new EditorConfiguration());
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

            fluentBuilder.BuildUpService(null, typeof(CrudService))
                .Name("crud")
                .Route(_configuration.ServiceBasePath + "crud/", new []{ Method.Get, Method.Post, Method.Put, Method.Delete }, 0)
                .CreateComponent("crudClient")
                .Build();

            // Load templates and accumulate all of the CSS and JS assets

            var script = new StringBuilder();
            var less = new StringBuilder();

            var liveUpdateLogTemplate = AddVueTemplate(script, less, "DispatcherLog");
            var pageEditorTemplate = AddVueTemplate(script, less, "PageEditor");

            // Load JavaScript modules and concatenate all the JavaScript

            var scriptModules = new List<string>();

            LoadScriptModule("dispatcherModule", scriptModules);
            LoadScriptModule("pagesModule", scriptModules);

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

            // This region of the CMS manager is for editing objects
            var editorRegion = Build(module, assetsComponent, fluentBuilder.BuildUpRegion()
                .Name("editor")
                .NeedsComponent("crudClient")
                .AddTemplate(pageEditorTemplate));

            // This region of the CMS manager shows changes as they happen
            var dispatcherlogRegion = Build(module, assetsComponent, fluentBuilder.BuildUpRegion()
                .Name("dispatcherLog")
                .NeedsComponent("liveUpdateClient")
                .AddTemplate(liveUpdateLogTemplate));

            // To have the CMS manager fill the whole page make this the page layout
            var managerLayout = fluentBuilder.BuildUpLayout()
                .Name("manager")
                .ZoneNesting("main,dispatcherLog")
                .Region("main", editorRegion)
                .Region("dispatcherLog", dispatcherlogRegion)
                .Build();

            // To have the CMS manager occupy a region of the page put this region
            // into a zone of the page layout
            var managerRegion = fluentBuilder.BuildUpRegion()
                .Name("manager")
                .Layout(managerLayout)
                .Build();

            // If the ManagerPath is configured then add a page to the website that
            // contains the CMS manager
            if (!string.IsNullOrEmpty(_configuration.ManagerPath))
            {
                fluentBuilder.BuildUpPage()
                    .Name("cmsManager")
                    .Route(_configuration.ManagerPath, 0, Method.Get)
                    .Layout(managerLayout)
                    .Build();
            }

            return this;
        }

        private IRegion Build(IModule module, IComponent assetsComponent, IRegionDefinition regionDefinition)
        {
            return regionDefinition
                .AssetDeployment(AssetDeployment.PerModule)
                .DeployIn(module)
                .NeedsComponent("libraries:Vue")
                .NeedsComponent("ajax:ajax")
                .NeedsComponent(assetsComponent)
                .Build();
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
                foreach (var line in markupLines)
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
