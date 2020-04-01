using System;
using System.Reflection;
using System.Text;
using Microsoft.Owin;
using OwinFramework.Interfaces.Utility;
using OwinFramework.MiddlewareHelpers.EmbeddedResources;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Restful.Interfaces;
using OwinFramework.Pages.Restful.Parameters;

namespace OwinFramework.Pages.Standard
{
    /// <summary>
    /// A service and a client-side javascript library that allows you to 
    /// dynamically add templates to your web pages on the client. Note that
    /// templates are sent directly to the browser and added to the DOM so
    /// data binding expressions within the template will not work.
    /// </summary>
    public class TemplatesPackage: IPackage
    {
        private readonly ResourceManager _resourceManager;
        private readonly TemplateLibraryComponent _templateLibraryComponent;
        private readonly TemplateFromUrlComponent _templateFromUrlComponent;

        public string NamespaceName { get; set; }
        public IModule Module { get; set; }
        public ElementType ElementType { get { return ElementType.Package; } }
        public string Name { get; set; }

        private string _servicePath;

        public TemplatesPackage(
            IFrameworkConfiguration frameworkConfiguration,
            IHostingEnvironment hostingEnvironment,
            IComponentDependenciesFactory componentDependencies)
        {
            Name = "templates";
            NamespaceName = "templates";

            _resourceManager = new ResourceManager(hostingEnvironment, new MimeTypeEvaluator());
            _templateLibraryComponent = new TemplateLibraryComponent(componentDependencies);
            _templateFromUrlComponent = new TemplateFromUrlComponent(componentDependencies);

            frameworkConfiguration.Subscribe(c =>
            {
                _servicePath = c.ServicesRootPath + "/templates";

                _templateLibraryComponent.Configure(
                    NamespaceName, 
                    _servicePath + "/template?path=");

                _templateFromUrlComponent.Configure(
                    new PathString(c.TemplateUrlRootPath),
                    new PathString(c.TemplateRootPath));
            });
        }

        IPackage IPackage.Build(IFluentBuilder fluentBuilder)
        {
            fluentBuilder.BuildUpService(null, typeof (TemplateService))
                .Name("templates")
                .Route(_servicePath, new[] { Method.Get }, 0)
                .Build();

            var templateLibraryJs = _resourceManager.GetResource(Assembly.GetExecutingAssembly(), "templateLibrary.js");
            if (templateLibraryJs.Content == null) return this;

            var javaScript = Encoding.UTF8.GetString(templateLibraryJs.Content);
            
           fluentBuilder.BuildUpComponent(_templateLibraryComponent)
                .Name("library")
                .AssetDeployment(AssetDeployment.PerWebsite)
                .DeployScript(javaScript)
                .Build();

            fluentBuilder.BuildUpComponent(_templateFromUrlComponent)
                 .Name("from_url")
                 .AssetDeployment(AssetDeployment.PerWebsite)
                 .Build();

            return this;
        }

        /// <summary>
        /// A component that loads and configures the template library JavaScript.
        /// This library has methods for dynamically fetching Html templates from
        /// a template service and adding the Html to a browser DOM element. This
        /// allows you to lazily load the Html for your page rather than delivering
        /// it all in the initial response.
        /// </summary>
        private class TemplateLibraryComponent : Html.Elements.Component
        {
            private string _javaScript;

            public TemplateLibraryComponent(IComponentDependenciesFactory dependencies) 
                : base(dependencies)
            {
                PageAreas = new []{ PageArea.Initialization };
            }

            public void Configure(string namespaceName, string templateServiceUrl)
            {
                var js = new StringBuilder();
                js.Append("ns.");
                js.Append(namespaceName);
                js.Append(".templateLibrary.init({templateServiceUrl: \"");
                js.Append(templateServiceUrl);
                js.Append("\"});");

                _javaScript = js.ToString();
            }

            public override IWriteResult WritePageArea(IRenderContext context, PageArea pageArea)
            {
                if (pageArea == PageArea.Initialization)
                {
                    context.Html.WriteScriptOpen();
                    context.Html.Write(_javaScript);
                    context.Html.WriteScriptClose();
                    context.Html.WriteLine();
                }
                return base.WritePageArea(context, pageArea);
            }
        }

        /// <summary>
        /// This component will render a template into the page on the server side where
        /// the path of the template is derrived from the url of the page. This provides
        /// a mechanism for creating a large number of pages where the content in one
        /// region of the page is defined by an html template. You can use any template 
        /// parser you like including the ones that provide a data binding syntax.
        /// </summary>
        private class TemplateFromUrlComponent: Html.Elements.Component
        {
            private PathString _urlBasePath;
            private PathString _templateBasePath;

            public TemplateFromUrlComponent(IComponentDependenciesFactory dependencies)
                : base(dependencies)
            {
                PageAreas = new[] 
                { 
                    PageArea.Head, 
                    PageArea.Title,
                    PageArea.Scripts,
                    PageArea.Styles,
                    PageArea.Body,
                    PageArea.Initialization 
                };

                _urlBasePath = new PathString("/");
                _templateBasePath = new PathString("/");
            }

            public void Configure(PathString urlBasePath, PathString templateBasePath)
            {
                _urlBasePath = urlBasePath;
                _templateBasePath = templateBasePath.HasValue ? templateBasePath : new PathString("/");
            }

            public override IWriteResult WritePageArea(IRenderContext context, PageArea pageArea)
            {
                var requestPath = context.OwinContext.Request.Path;
                PathString relativePath = requestPath;

                if (!_urlBasePath.HasValue || _urlBasePath.Value == "/" || requestPath.StartsWithSegments(_urlBasePath, out relativePath))
                {
                    var templatePath = _templateBasePath.Add(relativePath);
                    var template = Dependencies.NameManager.ResolveTemplate(templatePath.Value);

                    if (template != null)
                        return template.WritePageArea(context, pageArea);

                }

                return Html.Runtime.WriteResult.Continue();
            }
        }

        /// <summary>
        /// This service serves template files allowing them to be retrieved via AJAX and dynamically
        /// added to the page. The templateLibrary.js file contains the client-side code to make this
        /// easy.
        /// </summary>
        private class TemplateService
        {
            private readonly IAssetManager _assetmanager;
            private readonly INameManager _nameManager;
            private readonly IHtmlWriterFactory _htmlWriterFactory;
            private readonly IStringBuilderFactory _stringBuilderFactory;

            private IFrameworkConfiguration _frameworkConfiguration;

            public TemplateService(
                IAssetManager assetmanager,
                INameManager nameManager,
                IHtmlWriterFactory htmlWriterFactory,
                IStringBuilderFactory stringBuilderFactory,
                IFrameworkConfiguration frameworkConfiguration)
            {
                _assetmanager = assetmanager;
                _nameManager = nameManager;
                _htmlWriterFactory = htmlWriterFactory;
                _stringBuilderFactory = stringBuilderFactory;

                frameworkConfiguration.Subscribe(fc => _frameworkConfiguration = fc);
            }

            [Endpoint(UrlPath = "template", Methods = new[] {Method.Get}, ResponseSerializer = typeof (Restful.Serializers.Html))]
            [EndpointParameter("path", typeof (RequiredString), Description = "The path of the template to return")]
            private void GetTemplate(IEndpointRequest request)
            {
                var templatePath = request.Parameter<string>("path");
                var template = _nameManager.ResolveTemplate(templatePath);
                if (template == null)
                {
                    request.NotFound("Unknown template " + templatePath);
                }
                else
                {
                    using (var htmlWriter = _htmlWriterFactory.Create(_frameworkConfiguration))
                    {
                        var context = new TemplateRenderContext(_assetmanager, htmlWriter);
                        using (template.WritePageArea(context, PageArea.Body))
                        {
                            using (var html = _stringBuilderFactory.Create())
                            {
                                htmlWriter.ToStringBuilder(html);
                                request.Success(html.ToString());
                            }
                        }
                    }
                }
            }

            #region TemplateRenderContext

            private class TemplateRenderContext : IRenderContext
            {
                private readonly IAssetManager _assetManager;
                private readonly IHtmlWriter _htmlWriter;
                private bool _hasInitializationArea;

                public TemplateRenderContext(
                    IAssetManager assetManager,
                    IHtmlWriter htmlWriter)
                {
                    _assetManager = assetManager;
                    _htmlWriter = htmlWriter;
                }

                IRenderContext IRenderContext.Initialize(Microsoft.Owin.IOwinContext context)
                {
                    return this;
                }

                public void EnsureInitializationArea()
                {
                    if (!_hasInitializationArea)
                    {
                        _hasInitializationArea = true;
                        _htmlWriter.WriteScriptOpen();
                    }
                }

                public void EndInitializationArea()
                {
                    if (_hasInitializationArea)
                    {
                        _htmlWriter.WriteScriptClose();
                    }
                }

                void IRenderContext.Trace(System.Func<string> messageFunc)
                {
                }

                void IRenderContext.Trace<T>(System.Func<T, string> messageFunc, T arg)
                {
                }

                void IRenderContext.TraceIndent()
                {
                }

                void IRenderContext.TraceOutdent()
                {
                }

                IOwinContext IRenderContext.OwinContext
                {
                    get { throw new System.NotImplementedException(); }
                }

                IHtmlWriter IRenderContext.Html
                {
                    get { return _htmlWriter; }
                }

                string IRenderContext.Language
                {
                    get { return _assetManager.DefaultLanguage; }
                }

                bool IRenderContext.IncludeComments
                {
                    get { return false; }
                }

                Core.Interfaces.DataModel.IDataContext IRenderContext.Data
                {
                    get { throw new NotImplementedException(); }
                    set { }
                }

                void IRenderContext.AddDataContext(int id, Core.Interfaces.DataModel.IDataContext dataContext)
                {
                }

                Core.Interfaces.DataModel.IDataContext IRenderContext.GetDataContext(int id)
                {
                    throw new NotImplementedException();
                }

                void IRenderContext.SelectDataContext(int id)
                {
                    throw new NotImplementedException();
                }

                void IRenderContext.DeleteDataContextTree()
                {
                    throw new NotImplementedException();
                }
            }

            #endregion
        }
    }
}