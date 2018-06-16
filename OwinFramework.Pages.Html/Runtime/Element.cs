using System.Collections.Generic;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Runtime
{
    /// <summary>
    /// Base implementation of IPage. Inheriting from this olass will insulate you
    /// from any additions to the IPage interface
    /// </summary>
    public abstract class Element: IElement
    {
        private AssetDeployment _assetDeployment = AssetDeployment.Inherit;

        /// <summary>
        /// Gets or sets the asset deployment scheme for this element
        /// </summary>
        public virtual AssetDeployment AssetDeployment
        {
            get { return _assetDeployment; }
            set { _assetDeployment = value; }
        }

        /// <summary>
        /// Must be overriden in derrived classes to specify the element type
        /// </summary>
        public abstract ElementType ElementType { get; }

        /// <summary>
        /// A uniqie name for this page within the package
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Optional package that this page belongs to
        /// </summary>
        public virtual IPackage Package { get; set; }

        /// <summary>
        /// Optional module that this elements assets get deployed in
        /// </summary>
        public virtual IModule Module { get; set; }

        /// <summary>
        /// Override to output static assets
        /// </summary>
        public virtual IWriteResult WriteStaticAssets(AssetType assetType, IHtmlWriter writer)
        {
            return WriteResult.Continue();
        }

        /// <summary>
        /// Override to output dynamic assets
        /// </summary>
        public virtual IWriteResult WriteDynamicAssets(AssetType assetType, IHtmlWriter writer)
        {
            return WriteResult.Continue();
        }

        /// <summary>
        /// Override to output initialization script
        /// </summary>
        public virtual IWriteResult WriteInitializationScript(IRenderContext renderContext, IDataContext dataContext)
        {
            return WriteResult.Continue();
        }

        /// <summary>
        /// Override to output the page title
        /// </summary>
        public virtual IWriteResult WriteTitle(IRenderContext renderContext, IDataContext dataContext)
        {
            return WriteResult.Continue();
        }

        /// <summary>
        /// Override to output into the page head
        /// </summary>
        public virtual IWriteResult WriteHead(IRenderContext renderContext, IDataContext dataContext)
        {
            return WriteResult.Continue();
        }

        /// <summary>
        /// Override to output html
        /// </summary>
        public virtual IWriteResult WriteHtml(IRenderContext renderContext, IDataContext dataContext)
        {
            return WriteResult.Continue();
        }

        /// <summary>
        /// Provides a way to traverse the whole element tree
        /// </summary>
        public virtual IEnumerator<IElement> GetChildren()
        {
            return null;
        }

        /// <summary>
        /// Override this method to perform initialization steps after name resolution
        /// </summary>
        public virtual void Initialize(IInitializationData initializationData)
        {
            var assetDeployment = AssetDeployment == AssetDeployment.Inherit && Module != null
                ? Module.AssetDeployment
                : AssetDeployment;

            assetDeployment = AssetDeployment == AssetDeployment.Inherit
                ? initializationData.AssetDeployment
                : AssetDeployment;

            if (assetDeployment == AssetDeployment.PerModule && Module == null)
                assetDeployment = AssetDeployment.PerWebsite;

            initializationData.HasElement(this, assetDeployment, Module);

            var children = GetChildren();
            if (children == null) return;
            
            if (AssetDeployment != AssetDeployment.Inherit)
            {
                initializationData.Push();
                initializationData.AssetDeployment = AssetDeployment;
            }
            try
            {
                while (children.MoveNext())
                {
                    children.Current.Initialize(initializationData);
                }
            }
            finally
            {
                if (AssetDeployment != AssetDeployment.Inherit)
                {
                    initializationData.Pop();
                }
                children.Dispose();
            }
        }
    }
}
