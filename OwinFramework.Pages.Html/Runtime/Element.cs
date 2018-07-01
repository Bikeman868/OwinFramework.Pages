using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Runtime
{
    /// <summary>
    /// Base implementation of IPage. Inheriting from this olass will insulate you
    /// from any additions to the IPage interface
    /// </summary>
    public abstract class Element: IElement, IDataConsumer
    {
        private readonly IDataConsumer _dataConsumer;
        private AssetDeployment _assetDeployment = AssetDeployment.Inherit;
        private List<IComponent> _dependentComponents;

        protected Element(IDataConsumerFactory dataConsumerFactory)
        {
            _dataConsumer = dataConsumerFactory == null
                ? null
                : dataConsumerFactory.Create();
        }

        DebugElement IElement.GetDebugInfo() 
        {
            var debugInfo = new DebugElement();
            PopulateDebugInfo(debugInfo);
            return debugInfo;
        }

        protected void PopulateDebugInfo(DebugInfo debugInfo)
        {
            debugInfo.Name = Name;
            debugInfo.Instance = this;
        }

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
        /// Override to output static CSS
        /// </summary>
        public virtual IWriteResult WriteStaticCss(ICssWriter writer)
        {
            return WriteResult.Continue();
        }

        /// <summary>
        /// Override to output static CSS
        /// </summary>
        public virtual IWriteResult WriteStaticJavascript(IJavascriptWriter writer)
        {
            return WriteResult.Continue();
        }

        /// <summary>
        /// Override to output dynamic CSS
        /// </summary>
        public virtual IWriteResult WriteDynamicCss(ICssWriter writer, bool includeChildren)
        {
            var result = WriteResult.Continue();
            if (!includeChildren) return result;

            var children = GetChildren();
            if (children == null) return result;

            try
            {
                while (!result.IsComplete && children.MoveNext())
                {
                    result.Add(children.Current.WriteDynamicCss(writer));
                }
            }
            finally
            {
                children.Dispose();
            }

            return result;
        }

        /// <summary>
        /// Override to output dynamic Javascript
        /// </summary>
        public virtual IWriteResult WriteDynamicJavascript(IJavascriptWriter writer, bool includeChildren)
        {
            var result = WriteResult.Continue();
            if (!includeChildren) return result;

            var children = GetChildren();
            if (children == null) return result;

            try
            {
                while (!result.IsComplete && children.MoveNext())
                {
                    result.Add(children.Current.WriteDynamicJavascript(writer));
                }
            }
            finally
            {
                children.Dispose();
            }

            return result;
        }

        /// <summary>
        /// Override to output initialization script
        /// </summary>
        public virtual IWriteResult WriteInitializationScript(IRenderContext context, bool includeChildren)
        {
            var result = WriteResult.Continue();
            if (!includeChildren) return result;

            var children = GetChildren();
            if (children == null) return result;

            try
            {
                while (!result.IsComplete && children.MoveNext())
                {
                    result.Add(children.Current.WriteInitializationScript(context));
                }
            }
            finally
            {
                children.Dispose();
            }

            return result;
        }

        /// <summary>
        /// Override to output the page title
        /// </summary>
        public virtual IWriteResult WriteTitle(IRenderContext context, bool includeChildren)
        {
            var result = WriteResult.Continue();
            if (!includeChildren) return result;

            var children = GetChildren();
            if (children == null) return result;

            try
            {
                while (!result.IsComplete && children.MoveNext())
                {
                    result.Add(children.Current.WriteTitle(context));
                }
            }
            finally
            {
                children.Dispose();
            }

            return result;
        }

        /// <summary>
        /// Override to output into the page head
        /// </summary>
        public virtual IWriteResult WriteHead(IRenderContext context, bool includeChildren)
        {
            var result = WriteResult.Continue();
            if (!includeChildren) return result;

            var children = GetChildren();
            if (children == null) return result;

            try
            {
                while (!result.IsComplete && children.MoveNext())
                {
                    result.Add(children.Current.WriteHead(context));
                }
            }
            finally
            {
                children.Dispose();
            }

            return result;
        }

        /// <summary>
        /// Override to output html
        /// </summary>
        public virtual IWriteResult WriteHtml(IRenderContext context, bool includeChildren)
        {
            var result = WriteResult.Continue();
            if (!includeChildren) return result;

            var children = GetChildren();
            if (children == null) return result;

            try
            {
                while (!result.IsComplete && children.MoveNext())
                {
                    result.Add(children.Current.WriteHtml(context));
                }
            }
            finally
            {
                children.Dispose();
            }

            return result;
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

            assetDeployment = assetDeployment == AssetDeployment.Inherit
                ? initializationData.AssetDeployment
                : assetDeployment;

            if (assetDeployment == AssetDeployment.PerModule && Module == null)
                assetDeployment = AssetDeployment.PerWebsite;

            initializationData.HasElement(this, assetDeployment, Module);

            if (_dependentComponents != null)
            {
                foreach (var component in _dependentComponents)
                    initializationData.NeedsComponent(component);
            }

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

        public virtual void NeedsComponent(IComponent component)
        {
            if (_dependentComponents == null)
                _dependentComponents = new List<IComponent>();

            _dependentComponents.Add(component);
        }

        #region IDataConsumer

        void IDataConsumer.ResolveDependencies(IDataScopeProvider scopeProvider)
        {
            if (_dataConsumer == null) return;

            _dataConsumer.ResolveDependencies(scopeProvider);
        }

        void IDataConsumer.NeedsData<T>(string scopeName)
        {
            if (_dataConsumer == null) return;

            _dataConsumer.NeedsData<T>(scopeName);
        }

        void IDataConsumer.NeedsData(Type dataType, string scopeName)
        {
            if (_dataConsumer == null) return;

            _dataConsumer.NeedsData(dataType, scopeName);
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

        void IDataConsumer.NeedsProvider(IDataProvider dataProvider, IDataDependency dependency)
        {
            if (_dataConsumer == null) return;

            _dataConsumer.NeedsProvider(dataProvider, dependency);
        }

        #endregion
    }
}
