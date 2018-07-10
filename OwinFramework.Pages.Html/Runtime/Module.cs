using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Runtime
{
    /// <summary>
    /// Base implementation of IModule. Inheriting from this class will insulate you
    /// from any future additions to the IModule interface
    /// </summary>
    public class Module : IModule
    {
        public string Name { get; set; }

        private AssetDeployment _assetDeployment = AssetDeployment.PerModule;
        IModule IDeployable.Module { get; set; }

        public Module(IModuleDependenciesFactory dependencies)
        {
            // DO NOT change the method signature of this constructor as
            // this would break all modules in all applications that use
            // this framework!!
        }

        public DebugModule GetDebugInfo()
        {
            return new DebugModule
            {
                Name = Name,
                Instance = this,
            };
        }

        /// <summary>
        /// Gets or sets the asset deployment scheme for this element
        /// </summary>
        public virtual AssetDeployment AssetDeployment
        {
            get { return _assetDeployment; }
            set { _assetDeployment = value; }
        }


        public IWriteResult WriteStaticCss(ICssWriter writer)
        {
            return WriteResult.Continue();
        }

        public IWriteResult WriteStaticJavascript(IJavascriptWriter writer)
        {
            return WriteResult.Continue();
        }
    }
}
