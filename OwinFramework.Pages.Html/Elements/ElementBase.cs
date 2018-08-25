using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Elements
{
    /// <summary>
    /// Base class for all elements and page elements
    /// </summary>
    public abstract class ElementBase: IElement
    {
        private List<IComponent> _dependentComponents;

        public abstract AssetDeployment AssetDeployment { get; set; }
        public abstract ElementType ElementType { get; }
        public abstract string Name { get; set; }
        public abstract IPackage Package { get; set; }

        public IModule Module { get; set; }

        protected ElementBase()
        {
        }

        public DebugInfo GetDebugInfo() 
        {
            return PopulateDebugInfo(new DebugElement());
        }

        protected virtual DebugInfo PopulateDebugInfo(DebugInfo debugInfo)
        {
            debugInfo.Name = Name;
            debugInfo.Instance = this;

            var dataConsumer = GetDataConsumer();
            debugInfo.DataConsumer = ReferenceEquals(dataConsumer, null) 
                ? null
                : dataConsumer.GetDebugInfo();

            debugInfo.DependentComponents = _dependentComponents;

            return debugInfo;
        }

        public abstract IDataConsumer GetDataConsumer();

        public virtual void NeedsComponent(IComponent component)
        {
            if (_dependentComponents == null)
                _dependentComponents = new List<IComponent>();

            _dependentComponents.Add(component);
        }

        public virtual IEnumerator<IElement> GetChildren()
        {
            return null;
        }

        public override string ToString()
        {
            var description = ElementType.ToString().ToLower();
            
            description +=  " " + GetType().DisplayName(TypeExtensions.NamespaceOption.Ending);

            if (!string.IsNullOrEmpty(Name))
                description += " '" + Name + "'";

            return description;
        }

        public IEnumerable<IComponent> GetDependentComponents()
        {
            return _dependentComponents;
        }

        #region Writing HTML

        public abstract IWriteResult WriteStaticCss(ICssWriter writer);
        public abstract IWriteResult WriteStaticJavascript(IJavascriptWriter writer);
        public abstract IWriteResult WriteDynamicCss(ICssWriter writer, bool includeChildren);
        public abstract IWriteResult WriteDynamicJavascript(IJavascriptWriter writer, bool includeChildren);
        public abstract IWriteResult WriteInitializationScript(IRenderContext context, bool includeChildren);
        public abstract IWriteResult WriteTitle(IRenderContext context, bool includeChildren);
        public abstract IWriteResult WriteHead(IRenderContext context, bool includeChildren);
        public abstract IWriteResult WriteHtml(IRenderContext context, bool includeChildren);

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

            renderContext.TraceIndent();
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
                renderContext.TraceOutdent();
            }

            return writeResult;
        }

        protected IWriteResult WriteChildrenTitle(
            IWriteResult writeResult,
            IRenderContext renderContext)
        {
            var children = GetChildren();
            if (children == null) return writeResult;

            renderContext.TraceIndent();
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
                renderContext.TraceOutdent();
            }

            return writeResult;
        }

        protected IWriteResult WriteChildrenHead(
            IWriteResult writeResult,
            IRenderContext renderContext)
        {
            var children = GetChildren();
            if (children == null) return writeResult;

            renderContext.TraceIndent();
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
                renderContext.TraceOutdent();
            }

            return writeResult;
        }

        protected IWriteResult WriteChildrenHtml(
            IWriteResult writeResult,
            IRenderContext renderContext)
        {
            var children = GetChildren();
            if (children == null) return writeResult;

            renderContext.TraceIndent();
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
                renderContext.TraceOutdent();
            }

            return writeResult;
        }

        #endregion

        #region Initialization

        public abstract void Initialize(IInitializationData initializationData);

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

        protected virtual void InitializeDependants(IInitializationData initializationData)
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

            var dataConsumer = GetDataConsumer();
            if (dataConsumer != null)
                dataConsumer.AddDependenciesToScopeProvider(initializationData.ScopeProvider);
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
                initializationData.AssetDeployment = assetDeployment;
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

        #endregion
    }
}
