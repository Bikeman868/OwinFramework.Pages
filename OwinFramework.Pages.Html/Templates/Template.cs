using System;
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
    /// This class is used by template parsing engines to construct
    /// templates that can be registered with the template library. These
    /// templates capture snippets of html to write into various parts
    /// of the page using the AddXxx() methods. These are output to
    /// the pages at runtime using the WritePageArea() method.
    /// </summary>
    public class Template : ITemplate, IDataConsumer
    {
        string INamed.Name { get; set; }
        IPackage IPackagable.Package { get; set; }
        bool ITemplate.IsStatic { get; set; }

        ElementType INamed.ElementType { get { return ElementType.Template; } }

        private PageArea[] _pageAreas = new PageArea[0];

        private Action<IRenderContext>[] _headElements;
        private Action<IRenderContext>[] _styleElements;
        private Action<IRenderContext>[] _scriptElements;
        private Action<IRenderContext>[] _bodyElements;
        private Action<IRenderContext>[] _initializationElements;

        public Template(IDataConsumerFactory dataConsumerFactory)
        {
            _dataConsumer = dataConsumerFactory.Create();
        }

        public void Add(IEnumerable<Action<IRenderContext>> bodyElements)
        {
            if (_bodyElements == null)
                _bodyElements = bodyElements.ToArray();
            else
                _bodyElements = _bodyElements.Concat(bodyElements).ToArray();

            AddPageArea(PageArea.Body);
        }

        public void AddHead(IEnumerable<Action<IRenderContext>> headElements)
        {
            if (_headElements == null)
                _headElements = headElements.ToArray();
            else
                _headElements = _headElements.Concat(headElements).ToArray();

            AddPageArea(PageArea.Head);
        }

        public void AddScript(IEnumerable<Action<IRenderContext>> scriptElements)
        {
            if (_scriptElements == null)
                _scriptElements = scriptElements.ToArray();
            else
                _scriptElements = _scriptElements.Concat(scriptElements).ToArray();

            AddPageArea(PageArea.Scripts);
        }

        public void AddStyle(IEnumerable<Action<IRenderContext>> styleElements)
        {
            if (_styleElements == null)
                _styleElements = styleElements.ToArray();
            else
                _styleElements = _styleElements.Concat(styleElements).ToArray();

            AddPageArea(PageArea.Styles);
        }

        public void AddInitialization(IEnumerable<Action<IRenderContext>> initializationElements)
        {
            if (_initializationElements == null)
                _initializationElements = initializationElements.ToArray();
            else
                _initializationElements = _initializationElements.Concat(initializationElements).ToArray();

            AddPageArea(PageArea.Initialization);
        }

        IWriteResult ITemplate.WritePageArea(IRenderContext context, PageArea pageArea)
        {
            if (pageArea == PageArea.Head && _headElements != null)
            {
                for (var i = 0; i < _headElements.Length; i++)
                    _headElements[i](context);
            }

            if (pageArea == PageArea.Styles && _styleElements != null)
            {
                for (var i = 0; i < _styleElements.Length; i++)
                    _styleElements[i](context);
            }

            if (pageArea == PageArea.Scripts && _scriptElements != null)
            {
                for (var i = 0; i < _scriptElements.Length; i++)
                    _scriptElements[i](context);
            }

            if (pageArea == PageArea.Body && _bodyElements != null)
            {
                for (var i = 0; i < _bodyElements.Length; i++)
                    _bodyElements[i](context);
            }

            if (pageArea == PageArea.Initialization && _initializationElements != null)
            {
                for (var i = 0; i < _initializationElements.Length; i++)
                    _initializationElements[i](context);
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

        private void AddPageArea(PageArea pageArea)
        {
            if (_pageAreas.Contains(pageArea))
                return;

            _pageAreas = _pageAreas.Concat(Enumerable.Repeat(pageArea, 1)).ToArray();
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
