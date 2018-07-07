using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Framework.Managers
{
    internal class NameManager: INameManager
    {
        private readonly IDictionary<string, IComponent> _components;
        private readonly IDictionary<string, IRegion> _regions;
        private readonly IDictionary<string, ILayout> _layouts;
        private readonly IDictionary<string, IPage> _pages;
        private readonly IDictionary<string, IService> _services;
        private readonly IDictionary<string, IModule> _modules;
        private readonly IDictionary<string, IPackage> _packages;
        private readonly IDictionary<string, IDataProvider> _dataProviders;

        private readonly IList<PendingActionBase> _pendingActions;

        private readonly IDictionary<string, HashSet<string>> _assetNames;
        private readonly Random _random;

        public NameManager()
        {
            _pendingActions = new List<PendingActionBase>();

            _components = new Dictionary<string, IComponent>(StringComparer.InvariantCultureIgnoreCase);
            _regions = new Dictionary<string, IRegion>(StringComparer.InvariantCultureIgnoreCase);
            _layouts = new Dictionary<string, ILayout>(StringComparer.InvariantCultureIgnoreCase);
            _pages = new Dictionary<string, IPage>(StringComparer.InvariantCultureIgnoreCase);
            _services = new Dictionary<string, IService>(StringComparer.InvariantCultureIgnoreCase);
            _modules = new Dictionary<string, IModule>(StringComparer.InvariantCultureIgnoreCase);
            _packages = new Dictionary<string, IPackage>(StringComparer.InvariantCultureIgnoreCase);
            _dataProviders = new Dictionary<string, IDataProvider>(StringComparer.InvariantCultureIgnoreCase);

            _assetNames = new Dictionary<string, HashSet<string>>(StringComparer.InvariantCultureIgnoreCase);
            _random = new Random();
        }

        #region Name registration

        public void Register(IElement element)
        {
            if (string.IsNullOrEmpty(element.Name))
                element.Name = GenerateElementName(element.Package);
            else
                ValidateName(element.Name, element.Package);

            var name = element.Package == null 
                ? element.Name 
                : element.Package.NamespaceName + ":" + element.Name;

            var component = element as IComponent;
            if (component != null) lock (_components) _components.Add(name, component);

            var region = element as IRegion;
            if (region != null) lock (_regions) _regions.Add(name, region);

            var layout = element as ILayout;
            if (layout != null) lock (_layouts) _layouts.Add(name, layout);
        }

        public void Register(IRunable runable)
        {
            var page = runable as IPage;
            if (page != null)
            {
                if (string.IsNullOrEmpty(page.Name))
                    page.Name = GenerateElementName(page.Package);
                else
                    ValidateName(page.Name, page.Package);

                lock (_pages) _pages.Add(page.Name, page);
            }

            var service = runable as IService;
            if (service != null)
            {
                if (string.IsNullOrEmpty(service.Name))
                    service.Name = GenerateElementName(service.Package);
                else
                    ValidateName(service.Name, service.Package);

                lock (_services) _services.Add(service.Name, service);
            }
        }

        public void Register(IModule module)
        {
            if (string.IsNullOrEmpty(module.Name))
                module.Name = GenerateNamespaceName(String.Empty);

            lock (_modules) _modules[module.Name] = module;
        }

        public void Register(IPackage package)
        {
            if (string.IsNullOrEmpty(package.NamespaceName))
                package.NamespaceName = GenerateNamespaceName(String.Empty);
            else
                ValidateName(package.NamespaceName, null);

            if (string.IsNullOrEmpty(package.Name))
                package.Name = package.NamespaceName;

            lock(_packages) _packages[package.Name] = package;
        }

        public void Register(IDataProvider dataProvider)
        {
            if (string.IsNullOrEmpty(dataProvider.Name))
                dataProvider.Name = GenerateElementName(dataProvider.Package);
            else
                ValidateName(dataProvider.Name, dataProvider.Package);

            var name = dataProvider.Package == null
                ? dataProvider.Name
                : dataProvider.Package.NamespaceName + ":" + dataProvider.Name;

            lock (_dataProviders) _dataProviders[name] = dataProvider;
        }

        private void ValidateName(string name, IPackage package)
        {
            if (string.IsNullOrEmpty(name))
                return;

            if (char.IsDigit(name[0]) ||
                !name.All(c => char.IsDigit(c) || char.IsLetter(c) || c == '_'))
                throw new InvalidNameException(name, package);
        }

        #endregion

        #region Resolution handlers

        public void Bind()
        {
            Exception exception = null;

            lock (_pendingActions)
            {
                foreach (var action in _pendingActions)
                    try
                    {
                        action.Invoke();
                    }
                    catch (Exception ex)
                    {
                        if (exception == null) exception = ex;
                        System.Diagnostics.Trace.WriteLine("Exception during name resolution. " + ex.Message);
                    }
                _pendingActions.Clear();
            }

            if (exception != null) throw exception;

            foreach (var service in _services.Values)
            {
                try
                {
                    service.Initialize();
                }
                catch (Exception ex)
                {
                    if (exception == null) exception = ex;
                    System.Diagnostics.Trace.WriteLine("Exception initializing service '" + service.Name + "'. " + ex.Message);
                }
            }

            if (exception != null) throw exception;

            foreach (var page in _pages.Values)
            {
                try
                {
                    page.Initialize();
                }
                catch (Exception ex)
                {
                    if (exception == null) exception = ex;
                    System.Diagnostics.Trace.WriteLine("Exception initializing page '" + page.Name + "'. "+ ex.Message);
                }
            }

            if (exception != null) throw exception;
        }

        public void AddResolutionHandler(Action resolutionAction)
        {
            lock (_pendingActions)
                _pendingActions.Add(new PendingAction(resolutionAction));
        }

        public void AddResolutionHandler(Action<INameManager> resolutionAction)
        {
            lock (_pendingActions)
                _pendingActions.Add(new PendingAction<INameManager>(resolutionAction, this));
        }

        public void AddResolutionHandler<T>(Action<INameManager, T> resolutionAction, T context)
        {
            lock (_pendingActions)
                _pendingActions.Add(new PendingAction<INameManager, T>(resolutionAction, this, context));
        }

        private abstract class PendingActionBase
        {
            public abstract void Invoke();
        }

        private class PendingAction : PendingActionBase
        {
            private readonly Action _action;

            public PendingAction(Action action)
            {
                _action = action;
            }

            public override void Invoke()
            {
                _action();
            }
        }

        private class PendingAction<T> : PendingActionBase
        {
            private readonly Action<T> _action;
            private readonly T _param;

            public PendingAction(Action<T> action, T param)
            {
                _action = action;
                _param = param;
            }

            public override void Invoke()
            {
                _action(_param);
            }
        }

        private class PendingAction<T1, T2> : PendingActionBase
        {
            private readonly Action<T1, T2> _action;
            private readonly T1 _param1;
            private readonly T2 _param2;

            public PendingAction(Action<T1, T2> action, T1 param1, T2 param2)
            {
                _action = action;
                _param1 = param1;
                _param2 = param2;
            }

            public override void Invoke()
            {
                _action(_param1, _param2);
            }
        }

        #endregion

        #region Resolving names

        private T Resolve<T>(string name, IPackage package, IDictionary<string, T> names)
        {
            if (string.IsNullOrEmpty(name))
                throw new NameResolutionFailureException(typeof(T), package, "[blank]");

            var colonIndex = name.IndexOf(':');
            if (colonIndex >= 0)
            {
                T result;
                if (names.TryGetValue(name, out result))
                    return result;
                throw new NameResolutionFailureException(typeof(T), package, name);
            }

            if (package == null)
            {
                lock (names)
                {
                    T result;
                    if (names.TryGetValue(name, out result))
                        return result;
                }
            }
            else
            {
                var fqn = package.NamespaceName + ":" + name;

                lock (names)
                {
                    T result;
                    if (names.TryGetValue(fqn, out result))
                        return result;

                    if (names.TryGetValue(name, out result))
                        return result;
                }
            }
            throw new NameResolutionFailureException(typeof(T), package, name);
        }

        public IComponent ResolveComponent(string name, IPackage package)
        {
            return Resolve(name, package, _components);
        }

        public IRegion ResolveRegion(string name, IPackage package)
        {
            return Resolve(name, package, _regions);
        }

        public ILayout ResolveLayout(string name, IPackage package)
        {
            return Resolve(name, package, _layouts);
        }

        public IPage ResolvePage(string name, IPackage package)
        {
            return Resolve(name, package, _pages);
        }

        public IService ResolveService(string name, IPackage package = null)
        {
            return Resolve(name, package, _services);
        }

        public IDataProvider ResolveDataProvider(string name, IPackage package = null)
        {
            return Resolve(name, package, _dataProviders);
        }

        public IModule ResolveModule(string name)
        {
            lock (_modules)
            {
                IModule module;
                if (_modules.TryGetValue(name, out module))
                    return module;
            }
            throw new NameResolutionFailureException(typeof(IModule), null, name);
        }

        public IPackage ResolvePackage(string name)
        {
            lock (_packages)
            {
                IPackage package;
                if (_packages.TryGetValue(name, out package))
                    return package;
            }
            throw new NameResolutionFailureException(typeof(IPackage), null, name);
        }

        #endregion

        #region Name generation

        private string GenerateNamespaceName(string namespaceName)
        {
            HashSet<string> names;

            lock(_assetNames)
            {
                if (!_assetNames.TryGetValue(namespaceName, out names))
                {
                    names = new HashSet<string>();
                    _assetNames.Add(namespaceName, names);
                }
            }

            string name;
            bool available;
            var length = 15;

            do
            {
                var nameLength = length++ / 5;
                var chars = new char[nameLength];
                for (var i = 0; i < nameLength; i++)
                    chars[i] = (char)('a' + _random.Next(25));

                name = new string(chars);

                lock(names)
                    available = names.Add(name);

            } while (!available);

            return name;
        }

        public string GenerateAssetName(IElement element)
        {
            var assetName = (element.Package == null || string.IsNullOrEmpty(element.Package.NamespaceName))
                ? GenerateNamespaceName(string.Empty)
                : element.Package.NamespaceName + "_" + GenerateNamespaceName(element.Package.NamespaceName);

            return string.Intern(assetName);
        }

        public string GenerateElementName(IPackage package)
        {
            var elementName = (package == null || string.IsNullOrEmpty(package.NamespaceName))
                ? GenerateNamespaceName(string.Empty)
                : GenerateNamespaceName(package.NamespaceName);

            return string.Intern(elementName);
        }


        public void EnsureAssetName(IElement element, ref string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
                assetName = GenerateAssetName(element);
        }

        #endregion
    }
}
