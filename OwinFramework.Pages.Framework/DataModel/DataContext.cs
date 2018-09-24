using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        private IRenderContext _renderContext;
        private IDataContext _parent;

        public DataContext(
            IDictionaryFactory dictionaryFactory,
            DataContextFactory dataContextFactory,
            IDataDependencyFactory dataDependencyFactory)
        {
            _dataContextFactory = dataContextFactory;
            _dataDependencyFactory = dataDependencyFactory;
            _properties = new LinkedList<PropertyEntry>();
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

#if DEBUG
        /// <summary>
        /// This exists so that you can inspect it in the debugger. It serves no other purpose
        /// </summary>
        private string _debugProperties
        {
            get
            {
                var result = new StringBuilder();

                Action<DataContext> addProperties = dc =>
                    {
                        if (dc.DataContextBuilder != null)
                        {
                            result.AppendLine("Data context builder #" + dc.DataContextBuilder.Id);
                            var debuggable = dc.DataContextBuilder as IDebuggable;
                            if (debuggable != null)
                            {
                                var rules = debuggable.GetDebugInfo<DebugDataScopeRules>();
                                if (rules != null && rules.Scopes != null)
                                {
                                    foreach (var scope in rules.Scopes)
                                        result.AppendLine("   " + scope);
                                }
                            }
                        }

                        foreach(var property in dc._properties)
                        {
                            result.AppendFormat("{0} = {1}\n", property.ToString(), property.Value);
                        }
                    };

                result.AppendLine("Properties in this data context");
                addProperties(this);

                var parent = _parent as DataContext;
                while (parent != null)
                {
                    result.AppendLine();
                    result.AppendLine("Properties in parent data context");
                    addProperties(parent);
                    parent = parent._parent as DataContext;
                }

                return result.ToString();
            }
        }
#endif

        T IDebuggable.GetDebugInfo<T>(int parentDepth, int childDepth)
        {
            return new DebugDataContext
            {
                Instance = this,
                DataContextBuilder = DataContextBuilder,
                Properties = _properties.Select(p => p.Type).ToList(),
                Parent = _parent == null || parentDepth == 0 ? null : _parent.GetDebugInfo(parentDepth - 1, 0)
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
                SetProperty(type, scopeName, value);
            else
                _parent.Set(type, value, scopeName);
        }

        private void SetUnscoped(Type type, object value, int level)
        {
            if (level == 0 || _parent == null)
                SetProperty(type, value);
            else
            {
                _parent.Set(type, value, null, level - 1);
                DeleteProperty(type);
            }
        }

        private object GetScoped(Type type, string scopeName, bool isRequired)
        {
            var retry = false;
            while (true)
            {
                object result;
                if (TryGetProperty(type, scopeName, out result))
                    return result;

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
                if (TryGetProperty(type, out result))
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

        #region Properties

        private readonly LinkedList<PropertyEntry> _properties;

        private bool TryGetProperty(Type type, out object value)
        {
            PropertyEntry match = null;
            foreach(var property in _properties.Where(p => p.Type == type))
            {
                if (match == null)
                    match = property;
                else
                {
                    if (!string.IsNullOrEmpty(match.ScopeName))
                        match = property;
                }
            }

            if (match == null)
            {
                value = null;
                return false;
            }

            value = match.Value;
            return true;
        }

        private bool TryGetProperty(Type type, string scopeName, out object value)
        {
            if (string.IsNullOrEmpty(scopeName))
                return TryGetProperty(type, out value);

            var match = _properties.FirstOrDefault(
                p => p.Type == type && string.Equals(scopeName, p.ScopeName, StringComparison.OrdinalIgnoreCase));

            if (match == null)
            {
                value = null;
                return false;
            }

            value = match.Value;
            return true;
        }

        private void SetProperty(Type type, string scopeName, object value)
        {
            if (string.IsNullOrEmpty(scopeName))
            {
                SetProperty(type, value);
                return;
            }

            var existing = _properties.FirstOrDefault(
                p => p.Type == type && string.Equals(scopeName, p.ScopeName, StringComparison.OrdinalIgnoreCase));

            if (existing == null)
                _properties.AddFirst(new PropertyEntry { Type = type, ScopeName = scopeName, Value = value });
            else
                existing.Value = value;
        }

        private void SetProperty(Type type, object value)
        {
            var existing = _properties.FirstOrDefault(
                p => p.Type == type && string.IsNullOrEmpty(p.ScopeName));

            if (existing == null)
                _properties.AddFirst(new PropertyEntry { Type = type, Value = value });
            else
                existing.Value = value;
        }

        private void DeleteProperty(Type type, string scopeName)
        {
            if (string.IsNullOrEmpty(scopeName))
                DeleteProperty(type);
            else
            {
                // TODO: No efficient way to remove list elements using the .Net LinkedList class
            }
        }

        private void DeleteProperty(Type type)
        {
            // TODO: No efficient way to remove list elements using the .Net LinkedList class
        }

        private class PropertyEntry
        {
            public Type Type;
            public string ScopeName;
            public object Value;

            public override string ToString()
            {
                var result = Type.DisplayName(TypeExtensions.NamespaceOption.None);
                if (string.IsNullOrEmpty(ScopeName)) return result;
                return result + " in '" + ScopeName + "' scope";
            }
        }

        #endregion
    }
}
