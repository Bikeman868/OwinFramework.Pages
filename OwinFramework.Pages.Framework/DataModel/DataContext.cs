﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OwinFramework.Pages.Core.Collections;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Framework.DataModel
{
    internal class DataContext: ReusableObject, IDataContext, IDebuggable
    {
        public IDataContextBuilder DataContextBuilder { get; set; }

        private readonly DataContextFactory _dataContextFactory;
        private readonly IDataDependencyFactory _dataDependencyFactory;
        private readonly IThreadSafeDictionary<Type, object> _properties;
#if DEBUG
        private readonly int _id;
#endif

        private IRenderContext _renderContext;
        private IDataContext _parent;

        public DataContext(
            IDictionaryFactory dictionaryFactory,
            DataContextFactory dataContextFactory,
            IDataDependencyFactory dataDependencyFactory,
            IIdManager idManager)
        {
            _dataContextFactory = dataContextFactory;
            _dataDependencyFactory = dataDependencyFactory;
            _properties = dictionaryFactory.Create<Type, object>();
#if DEBUG
            _id = idManager.GetUniqueId();
#endif
        }

        public DataContext Initialize(
            Action<IReusable> disposeAction, 
            IRenderContext renderContext, 
            IDataContextBuilder dataContextBuilder,
            DataContext parent)
        {
            base.Initialize(disposeAction);

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
                            result.AppendLine("Data context builder #" + dc.DataContextBuilder.Id);

                        foreach(var type in dc._properties.Keys)
                        {
                            object value;
                            if (_properties.TryGetValue(type, out value))
                                result.AppendFormat("{0} = {1}\n", type.DisplayName(TypeExtensions.NamespaceOption.None), value);
                        }
                    };

                result.AppendLine("Properties in data context #"  + _id);
                addProperties(this);

                var parent = _parent as DataContext;
                while (parent != null)
                {
                    result.AppendLine();
                    result.AppendLine("Properties in parent data context #" + parent._id);
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
                Properties = _properties.Keys.ToList(),
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
        
        public void Set<T>(T value, string scopeName = null, int level = 0)
        {
            var type = typeof(T);

            if (level == 0 || _parent == null)
                _properties[type] = value;
            else
            {
                _parent.Set(value, scopeName, level - 1);
                _properties.Remove(type);
            }
        }

        void IDataContext.Set(Type type, object value, string scopeName, int level)
        {
            if (level == 0 || _parent == null)
                _properties[type] = value;
            else
            {
                _parent.Set(type, value, scopeName, level - 1);
                _properties.Remove(type);
            }
        }

        T IDataContext.Get<T>(string scopeName, bool isRequired)
        {
            return (T)((IDataContext)this).Get(typeof(T), scopeName, isRequired);
        }

        object IDataContext.Get(Type type, string scopeName, bool isRequired)
        {
            var isInScope = string.IsNullOrEmpty(scopeName);

            if (!isInScope && DataContextBuilder != null)
            {
                var dependency = _dataDependencyFactory.Create(type, scopeName);
                isInScope = DataContextBuilder.IsInScope(dependency);
            }

            if (isInScope)
            {
                var retry = false;
                while (true)
                {
                    object result;
                    if (_properties.TryGetValue(type, out result))
                        return result;

                    result = _parent == null ? null : _parent.Get(type, null, false);

                    if (result != null || !isRequired)
                        return result;

                    if (retry)
                    {
                        throw new Exception("This data context does not know how to find missing" +
                            " data of type " + type.DisplayName() + " because after adding it to the scope provider" +
                            " the dependency could still not be resolved");
                    }

                    if (DataContextBuilder == null)
                    {
                        if (_parent == null)
                            throw new Exception("This data context does not know how to find missing"+
                                " data of type " + type.DisplayName() + " because it does not have a scope provider");
                        return _parent.Get(type, scopeName);
                    }

                    DataContextBuilder.AddMissingData(_renderContext, _dataDependencyFactory.Create(type, scopeName));
                    retry = true;
                }
            }
            
            return _parent == null ? null : _parent.Get(type, scopeName, isRequired);
        }
    }
}
