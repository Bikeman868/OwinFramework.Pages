using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using OwinFramework.Interfaces.Utility;
using OwinFramework.MiddlewareHelpers.EmbeddedResources;
using OwinFramework.Pages.CMS.Manager.Configuration;
using OwinFramework.Pages.CMS.Manager.Services;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Templates;
using Urchin.Client.Interfaces;

namespace OwinFramework.Pages.CMS.Manager
{
    /// <summary>
    /// This package exports a layout and a region called "cms:manager" that
    /// you can refer to in your application to include the CMS manager into
    /// a page on your website.
    /// This package optionally adds a page to your website containing the
    /// CMS manager. You can enable this page by setting a URL path in the
    /// Urchin configuration.
    /// This package needs the Ajax package and the Templates package.
    /// </summary>
    public class CmsManagerPackage: IPackage
    {
        private readonly ITemplateBuilder _templateBuilder;
        private readonly INameManager _nameManager;
        private readonly ResourceManager _resourceManager;
        private readonly IDisposable _configNotification;

        public string NamespaceName { get; set; }
        public IModule Module { get; set; }
        public ElementType ElementType { get { return ElementType.Package; } }
        public string Name { get; set; }

        private ManagerConfiguration _configuration;

        public CmsManagerPackage(
            IHostingEnvironment hostingEnvironment,
            ITemplateBuilder templateBuilder,
            INameManager nameManager,
            IConfigurationStore configurationStore)
        {
            _templateBuilder = templateBuilder;
            _nameManager = nameManager;

            Name = "cms_manager";
            NamespaceName = "cmsmanager";
            _resourceManager = new ResourceManager(hostingEnvironment, new MimeTypeEvaluator());

            _configNotification = configurationStore.Register(
                ManagerConfiguration.Path, 
                c => _configuration = c.Sanitize(), 
                new ManagerConfiguration());
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

            fluentBuilder.BuildUpService(null, typeof(ListService))
                .Name("list")
                .Route(_configuration.ServiceBasePath + "list/", new []{ Method.Get, Method.Post }, 0)
                .CreateComponent("listClient")
                .Build();

            fluentBuilder.BuildUpService(null, typeof(HistoryService))
                .Name("history")
                .Route(_configuration.ServiceBasePath + "history/", new []{ Method.Get }, 0)
                .CreateComponent("historyClient")
                .Build();

            fluentBuilder.BuildUpService(null, typeof(SegmentTestingService))
                .Name("segmentTesting")
                .Route(_configuration.ServiceBasePath + "segment-testing/", new []{ Method.Get, Method.Post, Method.Put, Method.Delete }, 0)
                .CreateComponent("segmentTestingClient")
                .Build();

            // Load templates and accumulate all of the CSS and JS assets

            var scriptModules = new List<string>();

            LoadScriptModule("dispatcherModule", scriptModules);
            LoadScriptModule("dataModule", scriptModules);
            LoadScriptModule("validationModule", scriptModules);
            LoadScriptModule("viewsModule", scriptModules);
            LoadScriptModule("filtersModule", scriptModules);
            LoadScriptModule("componentsModule", scriptModules);

            // Load templates that are directly loaded into regions

            var less = new StringBuilder();

            var cmsManagerTemplatePath = AddTemplate("CmsManager", less, scriptModules);
            var debugToolsTemplatePath = AddTemplate("DebugTools", less, scriptModules);

            // Load Vue temlates that are dynamically constructed in JavaScript

            AddTemplate("EnvironmentSelector", less, scriptModules);
            AddTemplate("WebsiteVersionSelector", less, scriptModules);
            AddTemplate("UserSegmentSelector", less, scriptModules);
            AddTemplate("SegmentationScenarioSelector", less, scriptModules);
            AddTemplate("SegmentationTestSelector", less, scriptModules);
            AddTemplate("PageSelector", less, scriptModules);
            
            AddTemplate("UserSegmentDisplay", less, scriptModules);
            AddTemplate("ContextDisplay", less, scriptModules);

            AddTemplate("EnvironmentEditor", less, scriptModules);
            AddTemplate("WebsiteVersionEditor", less, scriptModules);
            AddTemplate("SegmentationScenarioEditor", less, scriptModules);
            AddTemplate("PageEditor", less, scriptModules);

            AddTemplate("DispatcherLog", less, scriptModules);

            // Output JavaScript and CSS assets in a module asset

            var module = fluentBuilder.BuildUpModule()
                .Name("cms_manager")
                .AssetDeployment(AssetDeployment.PerModule)
                .Build();

            var assetsComponentBuilder = fluentBuilder.BuildUpComponent(null)
                .Name("assets")
                .DeployIn(module)
                .RenderInitialization("cms-manager-init", "<script>setTimeout(ns." + NamespaceName + ".init, 10);</script>")
                .DeployLess(less.ToString());

            foreach (var scriptModule in scriptModules)
                assetsComponentBuilder.DeployScript(scriptModule);

            var assetsComponent = assetsComponentBuilder.Build();

            // This region of the CMS manager is for editing pages of the website
            var editorRegion = Build(module, assetsComponent, fluentBuilder.BuildUpRegion()
                .Name("editor")
                .NeedsComponent("crudClient")
                .NeedsComponent("listClient")
                .NeedsComponent("historyClient")
                .NeedsComponent("segmentTestingClient")
                .AddTemplate(cmsManagerTemplatePath));

            // This region of the CMS manager is for debug tools
            var toolsRegion = Build(module, assetsComponent, fluentBuilder.BuildUpRegion()
                .Name("tools")
                .AddTemplate(debugToolsTemplatePath));

            // To have the CMS manager fill the whole page make this the page layout
            var managerLayout = fluentBuilder.BuildUpLayout()
                .Name("manager")
                .ZoneNesting("editor,tools")
                .Region("editor", editorRegion)
                .Region("tools", toolsRegion)
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
                .NeedsComponent("templates:library")
                .NeedsComponent("liveUpdateClient")
                .NeedsComponent(assetsComponent)
                .Build();
        }

        private string AddTemplate(
            string baseName,
            StringBuilder less,
            List<string> modules)
        {
            LoadScriptModule(baseName, modules);

            var templateDefinition = _templateBuilder.BuildUpTemplate();

            var markupFileName = baseName + ".html";
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
