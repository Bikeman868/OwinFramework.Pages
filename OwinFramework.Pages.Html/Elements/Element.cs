using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Elements
{
    /// <summary>
    /// Base implementation of IPage. Inheriting from this olass will insulate you
    /// from any additions to the IPage interface
    /// </summary>
    public abstract class Element: ElementBase
    {
        private AssetDeployment _assetDeployment = AssetDeployment.Inherit;
        private string _name;
        private IPackage _package;
        private IModule _module;

        protected Element(IDataConsumerFactory dataConsumerFactory)
            : base(dataConsumerFactory)
        { }

        public override AssetDeployment AssetDeployment
        {
            get { return _assetDeployment; }
            set { _assetDeployment = value; }
        }

        public override string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public override IPackage Package
        {
            get { return _package; }
            set { _package = value; }
        }

        public override IModule Module
        {
            get { return _module; }
            set { _module = value; }
        }

        public override IWriteResult WriteStaticCss(ICssWriter writer)
        {
            return WriteResult.Continue();
        }

        public override IWriteResult WriteStaticJavascript(IJavascriptWriter writer)
        {
            return WriteResult.Continue();
        }

        public override IWriteResult WriteDynamicCss(ICssWriter writer, bool includeChildren)
        {
            var result = WriteResult.Continue();
            return includeChildren ? WriteChildrenDynamicCss(result, writer) : result;
        }

        public override IWriteResult WriteDynamicJavascript(IJavascriptWriter writer, bool includeChildren)
        {
            var result = WriteResult.Continue();
            return includeChildren ? WriteChildrenDynamicJavascript(result, writer) : result;
        }

        public override IWriteResult WriteInitializationScript(IRenderContext renderContext, bool includeChildren)
        {
            var result = WriteResult.Continue();
            return includeChildren ? WriteChildrenInitializationScript(result, renderContext) : result;
        }

        public override IWriteResult WriteTitle(IRenderContext renderContext, bool includeChildren)
        {
            var result = WriteResult.Continue();
            return includeChildren ? WriteChildrenTitle(result, renderContext) : result;
        }

        public override IWriteResult WriteHead(IRenderContext renderContext, bool includeChildren)
        {
            var result = WriteResult.Continue();
            return includeChildren ? WriteChildrenHead(result, renderContext) : result;
        }

        public override IWriteResult WriteHtml(IRenderContext renderContext, bool includeChildren)
        {
            var result = WriteResult.Continue();
            return includeChildren ? WriteChildrenHtml(result, renderContext) : result;
        }

        public override void Initialize(IInitializationData initializationData)
        {
            var assetDeployment = InitializeAssetDeployment(initializationData);
            initializationData.HasElement(this, assetDeployment, Module);

            InitializeDependants(initializationData);
            InitializeChildren(initializationData, assetDeployment);
        }
    }
}
