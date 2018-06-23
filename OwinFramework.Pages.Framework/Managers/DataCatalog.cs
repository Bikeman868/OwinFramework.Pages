using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Framework.Managers
{
    internal class DataCatalog: IDataCatalog
    {
        private readonly IDictionaryFactory _dictionaryFactory;
        private readonly HashSet<string> _assemblies;
        private readonly HashSet<string> _types;
        private readonly IThreadSafeDictionary<string, IDataProvider> _providersByName;
        private readonly IThreadSafeDictionary<string, IDictionary<Type, IDataProvider>> _providersByScope;
        private readonly IThreadSafeDictionary<Type, IDictionary<string, IDataProvider>> _providersByType;

        public DataCatalog(
            IDictionaryFactory dictionaryFactory)
        {
            _dictionaryFactory = dictionaryFactory;
            _assemblies = new HashSet<string>();
            _types = new HashSet<string>();
            _providersByName = dictionaryFactory.Create<string, IDataProvider>(StringComparer.InvariantCultureIgnoreCase);
            _providersByScope = dictionaryFactory.Create<string, IDictionary<Type, IDataProvider>>(StringComparer.InvariantCultureIgnoreCase);
            _providersByType = dictionaryFactory.Create<Type, IDictionary<string, IDataProvider>>();
        }

        public IDataCatalog Register(IDataProvider dataProvider)
        {
            if (dataProvider == null)
                return this;

            if (!_types.Add(dataProvider.GetType().FullName))
                return this;

            if (string.IsNullOrEmpty(dataProvider.Name))
                _providersByName.Add(dataProvider.Name, dataProvider);

            /*
            var scopes = dataProvider.Scopes;
            if (scopes != null)
            {
                foreach (var scope in scopes)
                {
                    var providers = _providersByScope.GetOrAdd(scope, s => _dictionaryFactory.Create<Type, IDataProvider>(), null);
                    providers.Add();
                }
            }
            */

            return this;
        }

        public IDataCatalog Register(Type dataProviderType, Func<Type, IDataProvider> factoryFunc)
        {
            if (_types.Contains(dataProviderType.FullName))
                return this;

            var dataProvider = factoryFunc(dataProviderType);
            return Register(dataProvider);
        }

        public IDataCatalog Register(Assembly assembly, Func<Type, IDataProvider> factoryFunc)
        {
            if (!_assemblies.Add(assembly.FullName))
                return this;

            var types = assembly.GetTypes();

            var dataProviderTypes = types.Where(t => t.GetCustomAttributes(true).Any(a => a is IsDataProviderAttribute));

            Exception exception = null;

            foreach (var providerType in dataProviderTypes)
            {
                try
                {
                    Register(providerType, factoryFunc);
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            }

            if (exception != null)
                throw exception;

            return this;
        }

        public T Ensure<T>(IDataContext dataContext) where T : class
        {
            return null;
        }
    }
}
