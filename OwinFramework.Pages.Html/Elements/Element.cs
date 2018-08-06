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
    /// Base implementation elements that are constructed and configured
    /// by the fluent builder.
    /// </summary>
    public abstract class Element : ElementBase, IDataConsumer
    {
        private readonly IDataConsumer _dataConsumer;

        private AssetDeployment _assetDeployment = AssetDeployment.Inherit;
        private string _name;
        private IPackage _package;
        private IModule _module;

        protected Element(IDataConsumerFactory dataConsumerFactory)
        {
            _dataConsumer = dataConsumerFactory == null
                ? null
                : dataConsumerFactory.Create();
        }

        public override IDataConsumer GetDataConsumer()
        {
            return _dataConsumer;
        }

        public override void Initialize(IInitializationData initializationData)
        {
            var assetDeployment = InitializeAssetDeployment(initializationData);
            initializationData.HasElement(this, assetDeployment, Module);

            InitializeDependants(initializationData);
            InitializeChildren(initializationData, assetDeployment);
        }

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

        #region Writing HTML

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

        #endregion

        #region IDataConsumer

        void IDataConsumer.HasDependency(IDataSupply dataSupply)
        {
            if (_dataConsumer == null) return;

            _dataConsumer.HasDependency(dataSupply);
        }

        IList<IDataSupply> IDataConsumer.AddDependenciesToScopeProvider(IDataScopeProvider dataScope)
        {
            if (_dataConsumer == null) return null;

            return _dataConsumer.AddDependenciesToScopeProvider(dataScope);
        }

        void IDataConsumer.HasDependency<T>(string scopeName)
        {
            if (_dataConsumer == null) return;

            _dataConsumer.HasDependency<T>(scopeName);
        }

        void IDataConsumer.HasDependency(Type dataType, string scopeName)
        {
            if (_dataConsumer == null) return;

            _dataConsumer.HasDependency(dataType, scopeName);
        }

        void IDataConsumer.CanUseData<T>(string scopeName)
        {
            if (_dataConsumer == null) return;

            _dataConsumer.CanUseData<T>(scopeName);
        }

        void IDataConsumer.CanUseData(Type dataType, string scopeName)
        {
            if (_dataConsumer == null) return;

            _dataConsumer.CanUseData(dataType, scopeName);
        }

        void IDataConsumer.HasDependency(IDataProvider dataProvider, IDataDependency dependency)
        {
            if (_dataConsumer == null) return;

            _dataConsumer.HasDependency(dataProvider, dependency);
        }

        DebugDataConsumer IDataConsumer.GetDebugInfo()
        {
            if (_dataConsumer == null) return null;
            return _dataConsumer.GetDebugInfo();
        }

        #endregion
    }
}
