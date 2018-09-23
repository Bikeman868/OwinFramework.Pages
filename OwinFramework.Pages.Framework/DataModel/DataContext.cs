using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Collections;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Framework.DataModel
{
    internal class DataContext: ReusableObject, IDataContext, IDebuggable
    {
        public IDataContextBuilder DataContextBuilder { get; set; }

        private readonly DataContextFactory _dataContextFactory;
        private readonly IDataDependencyFactory _dataDependencyFactory;
        private readonly IThreadSafeDictionary<Type, object> _properties;

        private IRenderContext _renderContext;
        private IDataContext _parent;

        public DataContext(
            IDictionaryFactory dictionaryFactory,
            DataContextFactory dataContextFactory,
            IDataDependencyFactory dataDependencyFactory)
        {
            _dataContextFactory = dataContextFactory;
            _dataDependencyFactory = dataDependencyFactory;
            _properties = dictionaryFactory.Create<Type, object>();
        }

        public DataContext Initialize(
            Action<IReusable> disposeAction, 
            IRenderContext renderContext, 
            IDataContextBuilder dataContextBuilder,
            DataContext parent)
        {
            if (dataContextBuilder == null)
                throw new ArgumentNullException("dataContextBuilder");

            if (renderContext == null)
                throw new ArgumentNullException("renderContext");

            Initialize(disposeAction);

            _properties.Clear();
            _renderContext = renderContext;
            _parent = parent;
            DataContextBuilder = dataContextBuilder;

            return this;
        }

        T IDebuggable.GetDebugInfo<T>(int parentDepth, int childDepth)
        {
            return new DebugDataContext
            {
                Instance = this,
                DataContextBuilder = DataContextBuilder,
                Properties = _properties.Keys.ToList(),
                Parent = parentDepth == 0 ? null : _parent.GetDebugInfo(parentDepth - 1, 0)
            } as T;
        }

        public override string ToString()
        {
            return DataContextBuilder == null
                ? "data context with " + _properties.Count + " properties"
                : "data context #" + DataContextBuilder.Id + " with " + _properties.Count + " properties";
        }

        IDataContext IDataContext.CreateChild(IDataContextBuilder dataContextBuilder)
        {
            return _dataContextFactory.Create(_renderContext, dataContextBuilder, this);
        }
        
        public void Set<T>(T value, string scopeName, int level)
        {
            if (string.IsNullOrEmpty(scopeName))
                SetUnscoped(typeof(T), value, level);
            else
                SetScoped(typeof(T), value, scopeName);
        }

        void IDataContext.Set(Type type, object value, string scopeName, int level)
        {
            if (string.IsNullOrEmpty(scopeName))
                SetUnscoped(type, value, level);
            else
                SetScoped(type, value, scopeName);
        }

        T IDataContext.Get<T>(string scopeName, bool isRequired)
        {
            if (string.IsNullOrEmpty(scopeName))
                return (T)GetUnscoped(typeof(T), isRequired);

            return (T)GetScoped(typeof(T), scopeName, isRequired);
        }

        object IDataContext.Get(Type type, string scopeName, bool isRequired)
        {
            if (string.IsNullOrEmpty(scopeName)) 
                return GetUnscoped(type, isRequired);

            return GetScoped(type, scopeName, isRequired);
        }

        private void SetScoped(Type type, object value, string scopeName)
        {
            var dependency = _dataDependencyFactory.Create(type, scopeName);
            var isInScope = DataContextBuilder.IsInScope(dependency);

            if (isInScope || _parent == null)
            {
                _properties[type] = value;
            }
            else
            {
                _parent.Set(type, value, scopeName);
            }
        }

        private void SetUnscoped(Type type, object value, int level)
        {
            if (level == 0 || _parent == null)
                _properties[type] = value;
            else
            {
                _parent.Set(type, value, null, level - 1);
                _properties.Remove(type);
            }
        }

        private object GetScoped(Type type, string scopeName, bool isRequired)
        {
            var dependency = _dataDependencyFactory.Create(type, scopeName);
            var isInScope = DataContextBuilder.IsInScope(dependency);

            if (!isInScope)
            {
                if (_parent == null)
                {
                    if (!isRequired) return null;
                    throw ScopeSearchFailedException(type, scopeName);
                }

                return _parent.Get(type, scopeName, isRequired);
            }

            var retry = false;
            while (true)
            {
                object result;
                if (_properties.TryGetValue(type, out result))
                    return result;

                if (!isRequired) return null;

                if (retry)
                    throw RetryFailedException(type);

                DataContextBuilder.AddMissingData(_renderContext, dependency);
                retry = true;
            }
        }

        private object GetUnscoped(Type type, bool isRequired)
        {
            var retry = false;
            while (true)
            {
                object result;
                if (_properties.TryGetValue(type, out result))
                    return result;

                if (_parent != null)
                    return _parent.Get(type, null, isRequired);

                if (!isRequired) return null;

                if (retry)
                    throw RetryFailedException(type);

                DataContextBuilder.AddMissingData(_renderContext, _dataDependencyFactory.Create(type));
                retry = true;
            }
        }

        private Exception RetryFailedException(Type type)
        {
            return new Exception("This data context does not know how to find missing" +
                " data of type " + type.DisplayName() + " because after adding it to the scope provider" +
                " the dependency could still not be resolved");
        }

        private Exception ScopeSearchFailedException(Type type, string scopeName)
        {
            return new Exception("This data context does not know how to find missing" +
                " data of type " + type.DisplayName() + " in '" + scopeName + "' scope because this type is" +
                " not in scope for any data conetxt in the ancestor path");
        }
    }
}
