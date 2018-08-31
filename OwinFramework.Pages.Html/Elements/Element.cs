using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Elements
{
    /// <summary>
    /// Base implementation elements that are constructed and configured
    /// by the fluent builder. You can also use this as a base class for
    /// your custom application elements but this is an advanced use case.
    /// </summary>
    public abstract class Element : IElement, IDebuggable
    {
        private readonly IDataConsumer _dataConsumer;
        private List<IComponent> _dependentComponents;

        public abstract ElementType ElementType { get; }

        public virtual AssetDeployment AssetDeployment { get; set; }
        public virtual string Name { get; set; }
        public virtual IPackage Package { get; set; }
        public virtual IModule Module { get; set; }

        protected Element(IDataConsumerFactory dataConsumerFactory)
        {
            _dataConsumer = dataConsumerFactory == null
                ? null
                : dataConsumerFactory.Create();
        }

        public DebugInfo GetDebugInfo(int parentDepth, int childDepth)
        {
            return PopulateDebugInfo(new DebugInfo(), parentDepth, childDepth);
        }

        protected virtual DebugInfo PopulateDebugInfo(DebugInfo debugInfo, int parentDepth, int childDepth)
        {
            debugInfo.Name = Name;
            debugInfo.Instance = this;

            debugInfo.DataConsumer = _dataConsumer.GetDebugInfo<DebugDataConsumer>();

            debugInfo.DependentComponents = _dependentComponents;

            return debugInfo;
        }

        public override string ToString()
        {
            var description = ElementType.ToString().ToLower();

            description += " " + GetType().DisplayName(TypeExtensions.NamespaceOption.Ending);

            if (!string.IsNullOrEmpty(Name))
                description += " '" + Name + "'";

            return description;
        }

        public virtual void NeedsComponent(IComponent component)
        {
            if (_dependentComponents == null)
                _dependentComponents = new List<IComponent>();

            _dependentComponents.Add(component);
        }

        public List<IComponent> GetDependentComponents()
        {
            return _dependentComponents;
        }

        public virtual IWriteResult WriteStaticCss(ICssWriter writer)
        {
            return WriteResult.Continue();
        }

        public virtual IWriteResult WriteStaticJavascript(IJavascriptWriter writer)
        {
            return WriteResult.Continue();
        }

        protected PageArea[] PageAreas = { PageArea.Body };

        public virtual IEnumerable<PageArea> GetPageAreas()
        {
            return PageAreas;
        }

        public virtual IWriteResult WriteInPageStyles(
            ICssWriter writer,
            Func<ICssWriter, IWriteResult, IWriteResult> childrenWriter)
        {
            return WriteResult.Continue();
        }

        public virtual IWriteResult WriteInPageFunctions(
            IJavascriptWriter writer,
            Func<IJavascriptWriter, IWriteResult, IWriteResult> childrenWriter)
        {
            return WriteResult.Continue();
        }

        #region IDataConsumer Mixin

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

        #endregion
    }
}
