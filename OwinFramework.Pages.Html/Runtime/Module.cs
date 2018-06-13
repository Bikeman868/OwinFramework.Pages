using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;

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

        /// <summary>
        /// Gets or sets the asset deployment scheme for this element
        /// </summary>
        public virtual AssetDeployment AssetDeployment
        {
            get { return _assetDeployment; }
            set { _assetDeployment = value; }
        }
    }
}
