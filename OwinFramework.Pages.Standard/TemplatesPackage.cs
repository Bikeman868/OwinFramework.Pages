using System.Reflection;
using System.Text;
using OwinFramework.Interfaces.Utility;
using OwinFramework.MiddlewareHelpers.EmbeddedResources;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Framework.Interfaces;
using OwinFramework.Pages.Restful.Interfaces;
using OwinFramework.Pages.Restful.Parameters;

namespace OwinFramework.Pages.Standard
{
    /// <summary>
    /// Returns HTML templates so that pages can be built dynamically on the browser
    /// </summary>
    public class TemplatesPackage: IPackage
    {
        private readonly ResourceManager _resourceManager;

        public string NamespaceName { get; set; }
        public IModule Module { get; set; }
        public ElementType ElementType { get { return ElementType.Package; } }
        public string Name { get; set; }

        private string _servicePath;

        public TemplatesPackage(
            IFrameworkConfiguration frameworkConfiguration,
            IHostingEnvironment hostingEnvironment)
        {
            Name = "templates";
            NamespaceName = "templates";

            _resourceManager = new ResourceManager(hostingEnvironment, new MimeTypeEvaluator());
            frameworkConfiguration.Subscribe(c => _servicePath = c.ServicesRootPath + "/templates");
        }

        IPackage IPackage.Build(IFluentBuilder fluentBuilder)
        {
            fluentBuilder.BuildUpService(null, typeof (TemplateService))
                .Name("templates")
                .Route(_servicePath, new[] { Method.Get }, 0)
                .CreateComponent("service")
                .Build();

            var resource = _resourceManager.GetResource(Assembly.GetExecutingAssembly(), "templatesLibrary.js");
            if (resource.Content == null) return this;

            var javaScript = Encoding.UTF8.GetString(resource.Content);

           fluentBuilder.BuildUpComponent(null)
                .Name("library")
                .AssetDeployment(AssetDeployment.PerWebsite)
                .DeployScript(javaScript)
                .Build();

            return this;
        }

        private class TemplateService
        {
            private readonly IAssetManager _assetmanager;
            private readonly INameManager _nameManager;
            private readonly IHtmlWriterFactory _htmlWriterFactory;
            private readonly IStringBuilderFactory _stringBuilderFactory;

            public TemplateService(
                IAssetManager assetmanager,
                INameManager nameManager,
                IHtmlWriterFactory htmlWriterFactory,
                IStringBuilderFactory stringBuilderFactory)
            {
                _assetmanager = assetmanager;
                _nameManager = nameManager;
                _htmlWriterFactory = htmlWriterFactory;
                _stringBuilderFactory = stringBuilderFactory;
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
                    using (var htmlWriter = _htmlWriterFactory.Create())
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

                Microsoft.Owin.IOwinContext IRenderContext.OwinContext
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
                    get { throw new System.NotImplementedException(); }
                    set { }
                }

                void IRenderContext.AddDataContext(int id, Core.Interfaces.DataModel.IDataContext dataContext)
                {
                }

                Core.Interfaces.DataModel.IDataContext IRenderContext.GetDataContext(int id)
                {
                    throw new System.NotImplementedException();
                }

                void IRenderContext.SelectDataContext(int id)
                {
                    throw new System.NotImplementedException();
                }

                void IRenderContext.DeleteDataContextTree()
                {
                    throw new System.NotImplementedException();
                }
            }

            #endregion
        }
    }
}