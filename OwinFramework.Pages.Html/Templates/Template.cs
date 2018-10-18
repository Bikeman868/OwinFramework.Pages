﻿using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Html.Templates
{
    /// <summary>
    /// This calss is only used by template parsing engines to construct
    /// templates that can be registered with the template library
    /// </summary>
    public class Template : ITemplate, IDataConsumer
    {
        string INamed.Name { get; set; }
        IPackage IPackagable.Package { get; set; }

        ElementType INamed.ElementType { get { return ElementType.Template; } }

        private readonly PageArea[] _pageAreas = { PageArea.Body };

        private Action<IRenderContext>[] _visualElements;

        public Template(IDataConsumerFactory dataConsumerFactory)
        {
            _dataConsumer = dataConsumerFactory.Create();
        }

        public void Add(IEnumerable<Action<IRenderContext>> visualElements)
        {
            if (_visualElements == null)
                _visualElements = visualElements.ToArray();
            else
                _visualElements = _visualElements.Concat(visualElements).ToArray();
        }

        IWriteResult ITemplate.WritePageArea(IRenderContext context, PageArea pageArea)
        {
            if (pageArea == PageArea.Body && _visualElements != null)
            {
                for (var i = 0; i < _visualElements.Length; i++)
                {
                    _visualElements[i](context);
                }
            }

            return WriteResult.Continue();
        }

        IEnumerable<PageArea> IPageWriter.GetPageAreas()
        {
            return _pageAreas;
        }

        IWriteResult IPageWriter.WriteInPageStyles(ICssWriter writer, Func<ICssWriter, IWriteResult, IWriteResult> childrenWriter)
        {
            return WriteResult.Continue();
        }

        IWriteResult IPageWriter.WriteInPageFunctions(IJavascriptWriter writer, Func<IJavascriptWriter, IWriteResult, IWriteResult> childrenWriter)
        {
            return WriteResult.Continue();
        }

        #region IDataConsumer mixin

        private readonly IDataConsumer _dataConsumer;

        void IDataConsumer.HasDependency<T>(string scopeName)
        {
            _dataConsumer.HasDependency<T>(scopeName);
        }

        void IDataConsumer.HasDependency(Type dataType, string scopeName)
        {
            _dataConsumer.HasDependency(dataType, scopeName);
        }

        void IDataConsumer.CanUseData<T>(string scopeName)
        {
            _dataConsumer.CanUseData<T>(scopeName);
        }

        void IDataConsumer.CanUseData(Type dataType, string scopeName)
        {
            _dataConsumer.CanUseData(dataType, scopeName);
        }

        void IDataConsumer.HasDependency(IDataSupplier dataSupplier, IDataDependency dependency)
        {
            _dataConsumer.HasDependency(dataSupplier, dependency);
        }

        void IDataConsumer.HasDependency(IDataSupply dataSupply)
        {
            _dataConsumer.HasDependency(dataSupply);
        }

        IDataConsumerNeeds IDataConsumer.GetConsumerNeeds()
        {
            return _dataConsumer.GetConsumerNeeds();
        }

        #endregion
    }
}
