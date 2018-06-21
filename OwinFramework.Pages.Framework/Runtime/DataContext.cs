using System;
using OwinFramework.Pages.Core.Collections;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Framework.Runtime
{
    internal class DataContext: ReusableObject, IDataContext
    {
        private readonly DataContextFactory _dataContextFactory;
        private readonly IThreadSafeDictionary<string, object> _properties;

        private DataContext _parent;

        public DataContext(
            IDictionaryFactory dictionaryFactory,
            DataContextFactory dataContextFactory)
        {
            _dataContextFactory = dataContextFactory;
            _properties = dictionaryFactory.Create<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public DataContext Initialize(Action<IReusable> disposeAction, DataContext parent)
        {
            base.Initialize(disposeAction);
            _properties.Clear();
            _parent = parent;
            return this;
        }

        public IDataContext CreateChild()
        {
            return _dataContextFactory.Create(this);
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
    
        public string this[string name]
        {
            get { return Get<string>(name, false); }
            set { Set(value, name, 0); }
        }
    }
}
