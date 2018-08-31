﻿using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Collections;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Framework.DataModel
{
    internal class DataContext: ReusableObject, IDataContext, IDebuggable
    {
        private readonly DataContextFactory _dataContextFactory;
        private readonly IDataDependencyFactory _dataDependencyFactory;
        private readonly IThreadSafeDictionary<Type, object> _properties;
        private readonly List<IDataProvider> _dataProviders = new List<IDataProvider>();

        private IRenderContext _renderContext;
        private IDataContext _parent;
        private IDataScopeProvider _scope;

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
            DataContext parent,
            IDataScopeProvider scope)
        {
            base.Initialize(disposeAction);
            _properties.Clear();
            _dataProviders.Clear();
            _renderContext = renderContext;
            _parent = parent;
            _scope = scope;
            return this;
        }

        DebugInfo IDebuggable.GetDebugInfo(int parentDepth, int childDepth)
        {
            return new DebugDataContext
            {
                Instance = this,
                Scope = _scope,
                Parent = _parent.GetDebugInfo(),
                Properties = _properties.Keys.ToList()
            };
        }

        IDataContext IDataContext.CreateChild(IDataScopeProvider scope)
        {
            return _dataContextFactory.Create(_renderContext, scope, this);
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

        void IDataContext.Set(Type type, object value, string scopeName = null, int level = 0)
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

            if (!isInScope && _scope != null)
            {
                var dependency = _dataDependencyFactory.Create(type, scopeName);
                isInScope = _scope.IsInScope(dependency);
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

                    if (_scope == null)
                    {
                        if (_parent == null)
                            throw new Exception("This data context does not know how to find missing"+
                                " data of type " + type.DisplayName() + " because it does not have a scope provider");
                        return _parent.Get(type, scopeName);
                    }

                    _scope.AddMissingData(_renderContext, _dataDependencyFactory.Create(type, scopeName));
                    retry = true;
                }
            }
            
            return _parent == null ? null : _parent.Get(type, scopeName, isRequired);
        }

        IDataScopeProvider IDataContext.Scope
        {
            get { return _scope ?? (_parent == null ? null : _parent.Scope); }
            set { _scope = value; }
        }
    }
}
