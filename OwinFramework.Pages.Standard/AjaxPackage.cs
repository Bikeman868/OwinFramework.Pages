using System.Reflection;
using System.Text;
using OwinFramework.Interfaces.Utility;
using OwinFramework.MiddlewareHelpers.EmbeddedResources;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;

namespace OwinFramework.Pages.Standard
{
    /// <summary>
    /// This package exports a JavaScript library for making Ajax
    /// calls to back-end services. If you use the Pages.Restful NuGet
    /// package in your solution and you configure your service endpoints 
    /// to generate JavaScript wrappers then you will need to  include 
    /// this package in your solution or write your own version of it.
    /// </summary>
    public class AjaxPackage: IPackage
    {
        private readonly ResourceManager _resourceManager;

        public string NamespaceName { get; set; }
        public IModule Module { get; set; }
        public ElementType ElementType { get { return ElementType.Package; } }
        public string Name { get; set; }

        /// <summary>
        /// Constructs a package that will build a component that includes a
        /// JavaScript library for making Ajax calls
        /// </summary>
        /// <param name="hostingEnvironment"></param>
        public AjaxPackage(IHostingEnvironment hostingEnvironment)
        {
            Name = "ajax";
            NamespaceName = "ajax";
            _resourceManager = new ResourceManager(hostingEnvironment, new MimeTypeEvaluator());
        }

        IPackage IPackage.Build(IFluentBuilder fluentBuilder)
        {
            var resource = _resourceManager.GetResource(Assembly.GetExecutingAssembly(), "restModule.js");
            if (resource.Content == null) return this;

            var javaScript = Encoding.UTF8.GetString(resource.Content);

            fluentBuilder.BuildUpComponent(null)
                .Name("ajax")
                .AssetDeployment(AssetDeployment.PerWebsite)
                .DeployScript(javaScript)
                .Build();

            return this;
        }
    }
}
