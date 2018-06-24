using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Collections;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Framework.Runtime
{
    internal class DataContext: ReusableObject, IDataContext
    {
        private readonly DataContextFactory _dataContextFactory;
        private readonly IDataCatalog _dataCatalog;
        private readonly IThreadSafeDictionary<string, object> _properties;
        private readonly List<IDataProvider> _dataProviders = new List<IDataProvider>();

        private IRenderContext _renderContext;
        private DataContext _parent;
        private string _scope;

        public DataContext(
            IDictionaryFactory dictionaryFactory,
            DataContextFactory dataContextFactory,
            IDataCatalog dataCatalog)
        {
            _dataContextFactory = dataContextFactory;
            _dataCatalog = dataCatalog;
            _properties = dictionaryFactory.Create<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public DataContext Initialize(
            Action<IReusable> disposeAction, 
            IRenderContext renderContext, 
            DataContext parent)
        {
            base.Initialize(disposeAction);
            _properties.Clear();
            _dataProviders.Clear();
            _renderContext = renderContext;
            _parent = parent;
            return this;
        }

        public IDataContext CreateChild()
        {
            return _dataContextFactory.Create(_renderContext, this);
        }
        
        public void Set<T>(T value, string name = null, int level = 0)
        {
            if (name == null)
                name = typeof(T).FullName;
            name = name.ToLower();
            if (level == 0 || _parent == null)
                _properties[name] = value;
            else
            {
                _parent.Set(value, name, level - 1);
                _properties.Remove(name);
            }
        }

        public T Get<T>(string name, bool isRequired)
        {
            if (name == null)
                name = typeof(T).FullName;

            object result;
            if (_properties.TryGetValue(name, out result)) return (T)result;

            return _parent != null ? _parent.Get<T>(name, isRequired) : default(T);
        }
    
        public object Get(Type type, string name = null, bool required = true)
        {
            throw new NotImplementedException();
        }

        public IList<object> GetMultiple(IList<Type> types)
        {
            throw new NotImplementedException();
        }

        public void Ensure(Type type)
        {
            throw new NotImplementedException();
        }

        public string this[string name]
        {
            get { return Get<string>(name, false); }
            set { Set(value, name, 0); }
        }

        public string Scope
        {
            get { return _scope ?? (_parent == null ? string.Empty : _parent.Scope); }
            set { _scope = value; }
        }

        public void Ensure(IDataProvider provider)
        {
            var context = this;
            do
            {
                if (context._dataProviders.Contains(provider)) return;
                context = context._parent;
            } while (context != null);

            _dataProviders.Add(provider);

            provider.EstablishContext(_renderContext, this, null);
        }
    }
}
