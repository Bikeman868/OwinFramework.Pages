using System.IO;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Facilities.Runtime
{
    /// <summary>
    /// Base implementation of IPage. Inheriting from this olass will insulate you
    /// from any additions to the IPage interface
    /// </summary>
    public class Element: IElement
    {
        /// <summary>
        /// A uniqie name for this page within the package
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Optional package that this page belongs to
        /// </summary>
        public virtual IPackage Package { get; set; }

        /// <summary>
        /// Override to output static assets
        /// </summary>
        public virtual IWriteResult WriteStaticAssets(AssetType assetType, IHtmlWriter writer)
        {
            return null;
        }

        /// <summary>
        /// Override to output dynamic assets
        /// </summary>
        public virtual IWriteResult WriteDynamicAssets(IRenderContext renderContext, IDataContext dataContext, AssetType assetType, IHtmlWriter writer)
        {
            return null;
        }

        /// <summary>
        /// Override to output initialization script
        /// </summary>
        public virtual IWriteResult WriteInitializationScript(IRenderContext renderContext, IDataContext dataContext, IHtmlWriter writer)
        {
            return null;
        }

        /// <summary>
        /// Override to output the page title
        /// </summary>
        public virtual IWriteResult WriteTitle(IRenderContext renderContext, IDataContext dataContext, IHtmlWriter writer)
        {
            return null;
        }

        /// <summary>
        /// Override to output into the page head
        /// </summary>
        public virtual IWriteResult WriteHead(IRenderContext renderContext, IDataContext dataContext, IHtmlWriter writer)
        {
            return null;
        }

        /// <summary>
        /// Override to output html
        /// </summary>
        public virtual IWriteResult WriteHtml(IRenderContext renderContext, IDataContext dataContext, IHtmlWriter writer)
        {
            return null;
        }
    }
}
