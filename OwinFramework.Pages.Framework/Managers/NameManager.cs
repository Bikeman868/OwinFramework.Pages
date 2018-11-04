using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Enums;
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
        private readonly IDictionary<string, ITemplate> _templates;

        private readonly List<PendingActionBase> _pendingActions;

        private readonly List<IElement> _pendingElementRegistrations;
        private readonly List<IDataProvider> _pendingDataProviderRegistrations;
        private readonly List<IRunable> _pendingRunableRegistrations;

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
            _templates = new Dictionary<string, ITemplate>(StringComparer.InvariantCultureIgnoreCase);

            _assetNames = new Dictionary<string, HashSet<string>>(StringComparer.InvariantCultureIgnoreCase);

            _pendingElementRegistrations = new List<IElement>();
            _pendingDataProviderRegistrations = new List<IDataProvider>();
            _pendingRunableRegistrations = new List<IRunable>();

            _random = new Random();
        }

        #region Name registration

        public void Register(IElement element)
        {
            if (string.IsNullOrEmpty(element.Name))
                element.Name = GenerateElementName(element.Package);
            else
                ValidateElementName(element.Name, element.Package);

            if (element.Package == null)
                _pendingElementRegistrations.Add(element);
            else
                RegisterElement(element);
        }

        public void Register(IRunable runable)
        {
            if (string.IsNullOrEmpty(runable.Name))
                runable.Name = GenerateElementName(runable.Package);
            else
                ValidateElementName(runable.Name, runable.Package);

            if (runable.Package == null)
                _pendingRunableRegistrations.Add(runable);
            else
                RegisterRunable(runable);
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
                ValidateElementName(package.NamespaceName, null);

            if (string.IsNullOrEmpty(package.Name))
                package.Name = package.NamespaceName;

            lock(_packages) _packages[package.Name] = package;
        }

        public void Register(ITemplate template, string path)
        {
            ValidatePath(path);

            lock (_templates) _templates[path] = template;
        }

        public void Register(IDataProvider dataProvider)
        {
            if (string.IsNullOrEmpty(dataProvider.Name))
                dataProvider.Name = GenerateElementName(dataProvider.Package);
            else
                ValidateElementName(dataProvider.Name, dataProvider.Package);

            if (dataProvider.Package == null)
                _pendingDataProviderRegistrations.Add(dataProvider);
            else
                RegisterDataProvider(dataProvider);
        }

        private void ValidateElementName(string name, IPackage package)
        {
            if (string.IsNullOrEmpty(name))
                return;

            if (char.IsDigit(name[0]) ||
                !name.All(c => char.IsDigit(c) || char.IsLetter(c) || c == '_'))
                throw new InvalidNameException(name, package);
        }

        private void ValidatePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new InvalidPathException(path);

            if (!path.All(c => char.IsDigit(c) || char.IsLetter(c) || c == '_' || c == '-' || c == '/' || c == '.'))
                throw new InvalidPathException(path);
        }

        private void RegisterElement(IElement element)
        {
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

        public void RegisterDataProvider(IDataProvider dataProvider)
        {
            var name = dataProvider.Package == null
                ? dataProvider.Name
                : dataProvider.Package.NamespaceName + ":" + dataProvider.Name;

            lock (_dataProviders) _dataProviders[name] = dataProvider;
        }

        public void RegisterRunable(IRunable runable)
        {
            var name = runable.Package == null
                ? runable.Name
                : runable.Package.NamespaceName + ":" + runable.Name;

            var page = runable as IPage;
            if (page != null)
                lock (_pages) _pages.Add(name, page);

            var service = runable as IService;
            if (service != null)
                lock (_services) _services.Add(name, service);
        }

        #endregion

        #region Resolution handlers

        public void Bind()
        {
            List<Exception> exceptions = null;

            if (_pendingElementRegistrations.Count > 0)
            {
                AddResolutionHandler(NameResolutionPhase.RegisterPackagedElements, 
                    () =>
                    {
                        foreach(var element in _pendingElementRegistrations)
                            RegisterElement(element);
                        _pendingElementRegistrations.Clear();
                    });
            }

            if (_pendingDataProviderRegistrations.Count > 0)
            {
                AddResolutionHandler(NameResolutionPhase.RegisterPackagedElements,
                    () =>
                    {
                        foreach (var dataProvider in _pendingDataProviderRegistrations)
                            RegisterDataProvider(dataProvider);
                        _pendingDataProviderRegistrations.Clear();
                    });
            }

            if (_pendingRunableRegistrations.Count > 0)
            {
                AddResolutionHandler(NameResolutionPhase.RegisterPackagedElements,
                    () =>
                    {
                        foreach (var runable in _pendingRunableRegistrations)
                            RegisterRunable(runable);
                        _pendingRunableRegistrations.Clear();
                    });
            }

            lock (_pendingActions)
            {
                _pendingActions.Sort();

                foreach (var action in _pendingActions)
                    try
                    {
                        action.Invoke();
                    }
                    catch (Exception ex)
                    {
                        if (exceptions == null)
                        {
                            exceptions = new List<Exception> { ex };
                        }
                        else
                        {
                            if (exceptions.Count < 10)
                                exceptions.Add(ex);
                        }
                        System.Diagnostics.Trace.WriteLine("Exception during name resolution. " + ex.Message);
                    }
                _pendingActions.Clear();
            }

            if (exceptions != null)
            {
                if (exceptions.Count == 1)
                    throw exceptions[0];
                throw new AggregateException("Multiple execptions were thrown during name resolution", exceptions);
            }
        }

        public void AddResolutionHandler(NameResolutionPhase phase, Action resolutionAction)
        {
            lock (_pendingActions)
                _pendingActions.Add(new PendingAction(phase, resolutionAction));
        }

        public void AddResolutionHandler(NameResolutionPhase phase, Action<INameManager> resolutionAction)
        {
            lock (_pendingActions)
                _pendingActions.Add(new PendingAction<INameManager>(phase, resolutionAction, this));
        }

        public void AddResolutionHandler<T>(NameResolutionPhase phase, Action<INameManager, T> resolutionAction, T context)
        {
            lock (_pendingActions)
                _pendingActions.Add(new PendingAction<INameManager, T>(phase, resolutionAction, this, context));
        }

        public void AddResolutionHandler<T1, T2>(NameResolutionPhase phase, Action<INameManager, T1, T2> resolutionAction, T1 param1, T2 param2)
        {
            lock (_pendingActions)
                _pendingActions.Add(new PendingAction<INameManager, T1, T2>(phase, resolutionAction, this, param1, param2));
        }

        private abstract class PendingActionBase : IComparable<PendingActionBase>
        {
            private readonly NameResolutionPhase _phase;
            public abstract void Invoke();

            protected PendingActionBase(NameResolutionPhase phase)
            {
                _phase = phase;
            }

            public int CompareTo(PendingActionBase other)
            {
                if (_phase == other._phase) return 0;
                return _phase > other._phase ? 1 : -1;
            }
        }

        private class PendingAction : PendingActionBase
        {
            private readonly Action _action;

            public PendingAction(NameResolutionPhase phase, Action action)
                : base(phase)
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

            public PendingAction(NameResolutionPhase phase, Action<T> action, T param)
                : base(phase)
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

            public PendingAction(NameResolutionPhase phase, Action<T1, T2> action, T1 param1, T2 param2)
                : base(phase)
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

        private class PendingAction<T1, T2, T3> : PendingActionBase
        {
            private readonly Action<T1, T2, T3> _action;
            private readonly T1 _param1;
            private readonly T2 _param2;
            private readonly T3 _param3;

            public PendingAction(NameResolutionPhase phase, Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3)
                : base(phase)
            {
                _action = action;
                _param1 = param1;
                _param2 = param2;
                _param3 = param3;
            }

            public override void Invoke()
            {
                _action(_param1, _param2, _param3);
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

        public ITemplate ResolveTemplate(string path)
        {
            lock (_templates)
            {
                ITemplate template;
                if (_templates.TryGetValue(path, out template))
                    return template;
            }
            return null;
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
