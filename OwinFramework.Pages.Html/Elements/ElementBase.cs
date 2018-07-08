using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Elements
{
    /// <summary>
    /// Base class for all elements and element instances
    /// </summary>
    public abstract class ElementBase: IElement, IDataConsumer
    {
        private readonly IDataConsumer _dataConsumer;
        private List<IComponent> _dependentComponents;

        public abstract AssetDeployment AssetDeployment { get; set; }
        public abstract ElementType ElementType { get; }
        public abstract string Name { get; set; }
        public abstract IPackage Package { get; set; }
        public abstract IModule Module { get; set; }

        protected ElementBase(IDataConsumerFactory dataConsumerFactory)
        {
            _dataConsumer = dataConsumerFactory == null
                ? null
                : dataConsumerFactory.Create();
        }

        DebugElement IElement.GetDebugInfo() 
        {
            var debugInfo = new DebugElement ();
            PopulateDebugInfo(debugInfo);
            return debugInfo;
        }

        protected virtual void PopulateDebugInfo(DebugInfo debugInfo)
        {
            debugInfo.Name = Name;
            debugInfo.Instance = this;
        }

        public abstract IWriteResult WriteStaticCss(ICssWriter writer);
        public abstract IWriteResult WriteStaticJavascript(IJavascriptWriter writer);
        public abstract IWriteResult WriteDynamicCss(ICssWriter writer, bool includeChildren);
        public abstract IWriteResult WriteDynamicJavascript(IJavascriptWriter writer, bool includeChildren);
        public abstract IWriteResult WriteInitializationScript(IRenderContext context, bool includeChildren);
        public abstract IWriteResult WriteTitle(IRenderContext context, bool includeChildren);
        public abstract IWriteResult WriteHead(IRenderContext context, bool includeChildren);
        public abstract IWriteResult WriteHtml(IRenderContext context, bool includeChildren);
        public abstract void Initialize(IInitializationData initializationData);

        public virtual IEnumerator<IElement> GetChildren()
        {
            return null;
        }

        protected IWriteResult WriteChildrenDynamicCss(
            IWriteResult writeResult, 
            ICssWriter writer)
        {
            var children = GetChildren();
            if (children == null) return writeResult;

            try
            {
                while (!writeResult.IsComplete && children.MoveNext())
                {
                    writeResult.Add(children.Current.WriteDynamicCss(writer));
                }
            }
            finally
            {
                children.Dispose();
            }

            return writeResult;
        }

        protected IWriteResult WriteChildrenDynamicJavascript(
            IWriteResult writeResult,
            IJavascriptWriter writer)
        {
            var children = GetChildren();
            if (children == null) return writeResult;

            try
            {
                while (!writeResult.IsComplete && children.MoveNext())
                {
                    writeResult.Add(children.Current.WriteDynamicJavascript(writer));
                }
            }
            finally
            {
                children.Dispose();
            }

            return writeResult;
        }

        protected IWriteResult WriteChildrenInitializationScript(
            IWriteResult writeResult,
            IRenderContext renderContext)
        {
            var children = GetChildren();
            if (children == null) return writeResult;

            try
            {
                while (!writeResult.IsComplete && children.MoveNext())
                {
                    writeResult.Add(children.Current.WriteInitializationScript(renderContext));
                }
            }
            finally
            {
                children.Dispose();
            }

            return writeResult;
        }

        protected IWriteResult WriteChildrenTitle(
            IWriteResult writeResult,
            IRenderContext renderContext)
        {
            var children = GetChildren();
            if (children == null) return writeResult;

            try
            {
                while (!writeResult.IsComplete && children.MoveNext())
                {
                    writeResult.Add(children.Current.WriteTitle(renderContext));
                }
            }
            finally
            {
                children.Dispose();
            }

            return writeResult;
        }

        protected IWriteResult WriteChildrenHead(
            IWriteResult writeResult,
            IRenderContext renderContext)
        {
            var children = GetChildren();
            if (children == null) return writeResult;

            try
            {
                while (!writeResult.IsComplete && children.MoveNext())
                {
                    writeResult.Add(children.Current.WriteHead(renderContext));
                }
            }
            finally
            {
                children.Dispose();
            }

            return writeResult;
        }

        protected IWriteResult WriteChildrenHtml(
            IWriteResult writeResult,
            IRenderContext renderContext)
        {
            var children = GetChildren();
            if (children == null) return writeResult;

            try
            {
                while (!writeResult.IsComplete && children.MoveNext())
                {
                    writeResult.Add(children.Current.WriteHtml(renderContext));
                }
            }
            finally
            {
                children.Dispose();
            }

            return writeResult;
        }

        protected AssetDeployment InitializeAssetDeployment(IInitializationData initializationData)
        {
            var assetDeployment = AssetDeployment == AssetDeployment.Inherit && Module != null
                ? Module.AssetDeployment
                : AssetDeployment;

            assetDeployment = assetDeployment == AssetDeployment.Inherit
                ? initializationData.AssetDeployment
                : assetDeployment;

            if (assetDeployment == AssetDeployment.PerModule && Module == null)
                assetDeployment = AssetDeployment.PerWebsite;

            return assetDeployment;
        }

        protected void InitializeDependants(IInitializationData initializationData)
        {
            if (_dependentComponents != null)
            {
                var skip = 0;
                do
                {
                    var newComponents = _dependentComponents.Skip(skip).ToList();

                    foreach (var component in newComponents)
                        component.Initialize(initializationData);

                    skip += newComponents.Count;
                } while (_dependentComponents.Count > skip);
            }

            if (_dataConsumer != null)
            {
                _dataConsumer.AddDependenciesToScopeProvider(initializationData.ScopeProvider);
            }
        }

        protected void InitializeChildren(
            IInitializationData initializationData,
            AssetDeployment assetDeployment)
        {
            var children = GetChildren();
            if (children == null) return;

            if (assetDeployment != AssetDeployment.Inherit)
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
                if (assetDeployment != AssetDeployment.Inherit)
                {
                    initializationData.Pop();
                }
                children.Dispose();
            }
        }

        public virtual void NeedsComponent(IComponent component)
        {
            if (_dependentComponents == null)
                _dependentComponents = new List<IComponent>();

            _dependentComponents.Add(component);
        }

        #region IDataConsumer

        void IDataConsumer.HasDependency(IDataSupply dataSupply)
        {
            if (_dataConsumer == null) return;

            _dataConsumer.HasDependency(dataSupply);
        }

        void IDataConsumer.AddDependenciesToScopeProvider(IDataScopeProvider dataScope)
        {
            if (_dataConsumer == null) return;

            _dataConsumer.AddDependenciesToScopeProvider(dataScope);
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

        #endregion
    }
}
