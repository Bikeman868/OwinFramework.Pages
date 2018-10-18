using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq.Modules;
using OwinFramework.Builder;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Mocks.Managers
{
    public class MockNameManager: ConcreteImplementationProvider<INameManager>, INameManager
    {
        public readonly IDictionary<string, IComponent> Components = new Dictionary<string, IComponent>();
        public readonly IDictionary<string, IRegion> Regions = new Dictionary<string, IRegion>();
        public readonly IDictionary<string, ILayout> Layouts = new Dictionary<string, ILayout>();
        public readonly IDictionary<string, IPage> Pages = new Dictionary<string, IPage>();
        public readonly IDictionary<string, IService> Services = new Dictionary<string, IService>();
        public readonly IDictionary<string, IModule> Modules = new Dictionary<string, IModule>();
        public readonly IDictionary<string, IPackage> Packages = new Dictionary<string, IPackage>();
        public readonly IDictionary<string, IDataProvider> DataProviders = new Dictionary<string, IDataProvider>();
        public readonly IDictionary<string, ITemplate> Templates = new Dictionary<string, ITemplate>();

        private readonly List<PendingActionBase> _pendingActions = new List<PendingActionBase>();

        protected override INameManager GetImplementation(IMockProducer mockProducer)
        {
            return this;
        }

        public void Bind()
        {
            foreach (var action in _pendingActions) action.Invoke();
            _pendingActions.Clear();

            foreach (var page in Pages.Values)
                page.Initialize();
        }

        public void Register(IElement element)
        {
            if (string.IsNullOrEmpty(element.Name))
                element.Name = Guid.NewGuid().ToShortString();

            var name = element.Package == null
                ? element.Name
                : element.Package.NamespaceName + ":" + element.Name;

            var component = element as IComponent;
            if (component != null) Components.Add(name, component);

            var region = element as IRegion;
            if (region != null) Regions.Add(name, region);

            var layout = element as ILayout;
            if (layout != null) Layouts.Add(name, layout);
        }

        public void Register(IRunable runable)
        {
            var page = runable as IPage;
            if (page != null)
            {
                if (string.IsNullOrEmpty(page.Name))
                    page.Name = Guid.NewGuid().ToShortString();

                Pages.Add(page.Name, page);
            }

            var service = runable as IService;
            if (service != null)
            {
                if (string.IsNullOrEmpty(service.Name))
                    service.Name = Guid.NewGuid().ToShortString();

                Services.Add(service.Name, service);
            }
        }

        public void Register(IModule module)
        {
            if (string.IsNullOrEmpty(module.Name))
                module.Name = Guid.NewGuid().ToShortString();

            Modules.Add(module.Name, module);
        }

        public void Register(IPackage package)
        {
            if (string.IsNullOrEmpty(package.Name))
                package.Name = Guid.NewGuid().ToShortString();

            Packages.Add(package.Name, package);
        }

        public void Register(IDataProvider dataProvider)
        {
            if (string.IsNullOrEmpty(dataProvider.Name))
                dataProvider.Name = Guid.NewGuid().ToShortString();

            var name = dataProvider.Package == null
                ? dataProvider.Name
                : dataProvider.Package.NamespaceName + ":" + dataProvider.Name;

            DataProviders[name] = dataProvider;
        }

        public void Register(ITemplate template, string path)
        {
            Templates[path] = template;
        }

        public void AddResolutionHandler(
            NameResolutionPhase phase, 
            Action resolutionAction)
        {
            _pendingActions.Add(new PendingAction(phase, resolutionAction));
        }

        public void AddResolutionHandler(
            NameResolutionPhase phase, 
            Action<INameManager> resolutionAction)
        {
            _pendingActions.Add(new PendingAction<INameManager>(phase, resolutionAction, this));
        }

        public void AddResolutionHandler<T>(
            NameResolutionPhase phase, 
            Action<INameManager, T> resolutionAction, 
            T context)
        {
            _pendingActions.Add(new PendingAction<INameManager, T>(phase, resolutionAction, this, context));
        }

        public void AddResolutionHandler<T1, T2>(
            NameResolutionPhase phase,
            Action<INameManager, T1, T2> resolutionAction,
            T1 element,
            T2 name)
        {
            _pendingActions.Add(new PendingAction<INameManager, T1, T2>(phase, resolutionAction, this, element, name));
        }

        public IComponent ResolveComponent(string name, IPackage package = null)
        {
            return Resolve(name, package, Components);
        }

        public IRegion ResolveRegion(string name, IPackage package = null)
        {
            return Resolve(name, package, Regions);
        }

        public ILayout ResolveLayout(string name, IPackage package = null)
        {
            return Resolve(name, package, Layouts);
        }

        public IPage ResolvePage(string name, IPackage package = null)
        {
            return Resolve(name, package, Pages);
        }

        public IService ResolveService(string name, IPackage package = null)
        {
            return Resolve(name, package, Services);
        }

        public IDataProvider ResolveDataProvider(string name, IPackage package = null)
        {
            return Resolve(name, package, DataProviders);
        }

        public ITemplate ResolveTemplate(string path)
        {
            return Templates[path];
        }

        public IModule ResolveModule(string name)
        {
            IModule module;
            if (Modules.TryGetValue(name, out module))
                return module;
            throw new NameResolutionFailureException(typeof(IModule), null, name);
        }

        public IPackage ResolvePackage(string name)
        {
            IPackage package;
            if (Packages.TryGetValue(name, out package))
                return package;
            throw new NameResolutionFailureException(typeof(IPackage), null, name);
        }

        public string GenerateAssetName(IElement element)
        {
            return Guid.NewGuid().ToShortString();
        }

        public void EnsureAssetName(IElement element, ref string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
                assetName = GenerateAssetName(element);
        }

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

        private abstract class PendingActionBase: IComparable<PendingActionBase>
        {
            private NameResolutionPhase _phase;

            protected PendingActionBase(NameResolutionPhase phase)
            {
                _phase = phase;
            }

            public abstract void Invoke();

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
    }
}
