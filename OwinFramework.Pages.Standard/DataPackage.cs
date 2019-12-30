using System.Reflection;
using System.Text;
using OwinFramework.Interfaces.Utility;
using OwinFramework.MiddlewareHelpers.EmbeddedResources;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Standard
{
    /// <summary>
    /// This package exports a JavaScript library for creating data stores
    /// within your JavaScript code. This package depends on the Ajax package
    /// and assumes that you have created a back-end service that performs
    /// CRUD operations for each store.
    /// For an example of how to use this library, see the dataModule.js file
    /// in the OwinFramework.Pages.CMS.Manager project from this solution.
    /// </summary>
    public class DataPackage: IPackage
    {
        private readonly ResourceManager _resourceManager;

        string IPackage.NamespaceName { get; set; }
        IModule IPackage.Module { get; set; }
        ElementType INamed.ElementType { get { return ElementType.Package; } }
        string INamed.Name { get; set; }

        /// <summary>
        /// Constructs a package that will build a component that includes a
        /// JavaScript library for managing data stores
        /// </summary>
        /// <param name="hostingEnvironment"></param>
        public DataPackage(IHostingEnvironment hostingEnvironment)
        {
            var my = this as IPackage;
            my.Name = "data";
            my.NamespaceName = "data";

            _resourceManager = new ResourceManager(hostingEnvironment, new MimeTypeEvaluator());
        }

        IPackage IPackage.Build(IFluentBuilder fluentBuilder)
        {
            var resource = _resourceManager.GetResource(Assembly.GetExecutingAssembly(), "dataModule.js");
            if (resource.Content == null) return this;

            var javaScript = Encoding.UTF8.GetString(resource.Content);

           fluentBuilder.BuildUpComponent(null)
                .Name("data")
                .AssetDeployment(AssetDeployment.PerWebsite)
                .DeployScript(javaScript)
                .Build();

            return this;
        }
    }
}
