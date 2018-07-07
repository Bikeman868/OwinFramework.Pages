using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Elements
{
    internal abstract class ElementInstance<T> : ElementBase where T : IElement
    {
        public bool IsInstance { get { return true; } }

        protected readonly T Parent;
        private AssetDeployment _assetDeployment = AssetDeployment.Inherit;

        protected ElementInstance(
            IDataConsumerFactory dataConsumerFactory,
            T parent)
            : base(dataConsumerFactory)
        {
            if (parent == null)
                throw new ArgumentNullException("parent");

            Parent = parent;
        }

        public override AssetDeployment AssetDeployment
        {
            get { return _assetDeployment == AssetDeployment.Inherit ? Parent.AssetDeployment : _assetDeployment; }
            set { _assetDeployment = value; }
        }

        public override string Name
        {
            get { return Parent.Name; }
            set { throw new InvalidOperationException("You can not name an instance " + typeof(T).DisplayName()); }
        }

        public override IPackage Package
        {
            get { return Parent.Package; }
            set { throw new InvalidOperationException("You can not set the package for an instance " + typeof(T).DisplayName()); }
        }

        public override IModule Module
        {
            get { return Parent.Module; }
            set { throw new InvalidOperationException("You can not set the module for an instance " + typeof(T).DisplayName()); }
        }

        public override IWriteResult WriteStaticCss(ICssWriter writer)
        {
            return Parent.WriteStaticCss(writer);
        }

        public override IWriteResult WriteStaticJavascript(IJavascriptWriter writer)
        {
            return Parent.WriteStaticJavascript(writer);
        }

        public override IWriteResult WriteDynamicCss(ICssWriter writer, bool includeChildren)
        {
            var result = Parent.WriteDynamicCss(writer, false);
            return includeChildren ? WriteChildrenDynamicCss(result, writer) : result;
        }

        public override IWriteResult WriteDynamicJavascript(IJavascriptWriter writer, bool includeChildren)
        {
            var result = Parent.WriteDynamicJavascript(writer, false);
            return includeChildren ? WriteChildrenDynamicJavascript(result, writer) : result;
        }

        public override IWriteResult WriteInitializationScript(IRenderContext renderContext, bool includeChildren)
        {
            var result = Parent.WriteInitializationScript(renderContext, false);
            return includeChildren ? WriteChildrenInitializationScript(result, renderContext) : result;
        }

        public override IWriteResult WriteTitle(IRenderContext renderContext, bool includeChildren)
        {
            var result = Parent.WriteTitle(renderContext, false);
            return includeChildren ? WriteChildrenTitle(result, renderContext) : result;
        }

        public override IWriteResult WriteHead(IRenderContext renderContext, bool includeChildren)
        {
            var result = Parent.WriteHead(renderContext, false);
            return includeChildren ? WriteChildrenHead(result, renderContext) : result;
        }

        public override void Initialize(IInitializationData initializationData)
        {
            var assetDeployment = InitializeAssetDeployment(initializationData);
            initializationData.HasElement(Parent, assetDeployment, Parent.Module);

            InitializeDependants(initializationData);
            InitializeChildren(initializationData, assetDeployment);
        }

    }
}
