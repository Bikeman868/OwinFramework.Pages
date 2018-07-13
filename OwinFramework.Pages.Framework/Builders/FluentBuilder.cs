using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.DataModel;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Core.RequestFilters;

namespace OwinFramework.Pages.Framework.Builders
{
    /// <summary>
    /// Uses reflection to find pages, layouts, regions, components
    /// and services in the solution and registers then and wires
    /// them together
    /// </summary>
    internal class FluentBuilder : IFluentBuilder
    {
        public IModuleBuilder ModuleBuilder { get; set; }
        public IPageBuilder PageBuilder { get; set; }
        public ILayoutBuilder LayoutBuilder { get; set; }
        public IRegionBuilder RegionBuilder { get; set; }
        public IComponentBuilder ComponentBuilder { get; set; }
        public IServiceBuilder ServiceBuilder { get; set; }
        public IPackageBuilder PackageBuilder { get; set; }
        public IDataProviderBuilder DataProviderBuilder { get; set; }

        private readonly INameManager _nameManager;
        private readonly IDataCatalog _dataCatalog;
        private readonly IDataDependencyFactory _dataDependencyFactory;
        private readonly IDataSupplierFactory _dataSupplierFactory;
        private readonly HashSet<string> _assemblies;
        private readonly HashSet<Type> _types;
        private readonly IPackage _packageContext;

        public FluentBuilder(
            INameManager nameManager,
            IDataCatalog dataCatalog,
            IDataDependencyFactory dataDependencyFactory,
            IDataSupplierFactory dataSupplierFactory)
        {
            _nameManager = nameManager;
            _dataCatalog = dataCatalog;
            _dataDependencyFactory = dataDependencyFactory;
            _dataSupplierFactory = dataSupplierFactory;
            _assemblies = new HashSet<string>();
            _types = new HashSet<Type>();
        }

        private FluentBuilder(
            FluentBuilder parent,
            IPackage packageContext,
            IDataCatalog dataCatalog,
            IDataDependencyFactory dataDependencyFactory,
            IDataSupplierFactory dataSupplierFactory)
        {
            _nameManager = parent._nameManager;
            _assemblies = parent._assemblies;
            _types = parent._types;
            _packageContext = packageContext;
            _dataCatalog = dataCatalog;
            _dataDependencyFactory = dataDependencyFactory;
            _dataSupplierFactory = dataSupplierFactory;

            ModuleBuilder = parent.ModuleBuilder;
            PageBuilder = parent.PageBuilder;
            LayoutBuilder = parent.LayoutBuilder;
            RegionBuilder = parent.RegionBuilder;
            ComponentBuilder = parent.ComponentBuilder;
            ServiceBuilder = parent.ServiceBuilder;
            PackageBuilder = parent.PackageBuilder;
            DataProviderBuilder = parent.DataProviderBuilder;
        }

        #region Register a package overriding the namespace

        IPackage IFluentBuilder.Register(IPackage package, string namespaceName)
        {
            var type = package.GetType();
            if (!_types.Add(type)) return package;

            return BuildPackage(package, type, null, namespaceName);
        }

        #endregion

        #region Enumerating types in an assembly

        void IFluentBuilder.Register(Assembly assembly, Func<Type, object> factory)
        {
            if (!_assemblies.Add(assembly.FullName))
                return;

            var types = assembly.GetTypes();

            var packageTypes = new List<Type>();
            var dataProviderTypes = new List<Type>();
            var otherTypes = new List<Type>();

            foreach(var type in types)
            {
                var customAttributes = type.GetCustomAttributes(true);
                if (customAttributes.Any(a => a is IsPackageAttribute))
                    packageTypes.Add(type);
                else if (customAttributes.Any(a => a is IsDataProviderAttribute))
                    dataProviderTypes.Add(type);
                else if (customAttributes.Length > 0)
                    otherTypes.Add(type);
            }

            var  exceptions = new List<Exception>();

            Action<Type> register = t =>
                {
                    try
                    {
                        ((IFluentBuilder)this).Register(t, factory);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                };

            foreach (var type in packageTypes) register(type);
            foreach (var type in dataProviderTypes) register(type);
            foreach (var type in otherTypes) register(type);

            if (exceptions.Count == 1)
                throw exceptions[0];

            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);
        }

        #endregion

        #region Registering types that use attributes to define elements declaratively

        object IFluentBuilder.Register(Type type, Func<Type, object> factory)
        {
            if (!_types.Add(type)) return null;

            var attributes = new AttributeSet(type);

            if (attributes.IsPackage != null) return BuildPackage(null, type, factory, null);
            if (attributes.IsModule != null) return BuildModule(type, factory);
            if (attributes.IsPage != null) return BuildPage(type, factory);
            if (attributes.IsLayout != null) return BuildLayout(type, factory);
            if (attributes.IsRegion != null) return BuildRegion(type, factory);
            if (attributes.IsComponent != null) return BuildComponent(type, factory);
            if (attributes.IsService != null) return BuildService(type, factory);
            if (attributes.IsDataProvider != null) return BuildDataProvider(type, factory);

            return null;
        }

        #endregion

        #region Registering pre-built elements

        /// <summary>
        /// This method is called from the ElementDefinition.Build() method 
        /// when the element is completely built and configured. It can also be
        /// called at application startup to register custom types that were built
        /// by the application
        /// </summary>
        void IFluentBuilder.Register(object obj)
        {
            var package = obj as IPackage;
            if (package != null)
                _nameManager.Register(package);

            var module = obj as IModule;
            if (module != null)
                _nameManager.Register(module);

            var element = obj as IElement;
            if (element != null)
                _nameManager.Register(element);

            var dataProvider = obj as IDataProvider;
            if (dataProvider != null)
                _nameManager.Register(dataProvider);

            var dataSupplier = obj as IDataSupplier;
            if (dataSupplier != null)
                _dataCatalog.Register(dataSupplier);
        }

        #endregion

        #region Entry points for fluently building various types of element

        /// <summary>
        /// Calls the component builder plug-in to return a fluent interface for configuring a component.
        /// You can pass a component instance or allow the builder to build one from scratch
        /// </summary>
        /// <param name="componentInstance">Optional instance to configure</param>
        /// <param name="declaringType">Used to configure the instance from custom attributes</param>
        /// <returns></returns>
        public IComponentDefinition BuildUpComponent(object componentInstance, Type declaringType, IPackage package)
        {
            if (ComponentBuilder == null)
                throw new FluentBuilderException("There is no build engine installed that knows how to build components");
            return ComponentBuilder.BuildUpComponent(componentInstance, declaringType, package ?? _packageContext);
        }

        /// <summary>
        /// Calls the region builder plug-in to return a fluent interface for configuring a region.
        /// You can pass a region instance or allow the builder to build one from scratch
        /// </summary>
        /// <param name="regionInstance">Optional instance to configure</param>
        /// <param name="declaringType">Used to configure the instance from custom attributes</param>
        /// <returns></returns>
        public IRegionDefinition BuildUpRegion(object regionInstance, Type declaringType, IPackage package)
        {
            if (RegionBuilder == null)
                throw new FluentBuilderException("There is no build engine installed that knows how to build regions");
            return RegionBuilder.BuildUpRegion(regionInstance, declaringType, package ?? _packageContext);
        }

        /// <summary>
        /// Calls the layout builder plug-in to return a fluent interface for configuring a layout.
        /// You can pass a layout instance or allow the builder to build one from scratch
        /// </summary>
        /// <param name="layoutInstance">Optional instance to configure</param>
        /// <param name="declaringType">Used to configure the instance from custom attributes</param>
        /// <returns></returns>
        public ILayoutDefinition BuildUpLayout(object layoutInstance, Type declaringType, IPackage package)
        {
            if (LayoutBuilder == null)
                throw new FluentBuilderException("There is no build engine installed that knows how to build layouts");
            return LayoutBuilder.BuildUpLayout(layoutInstance, declaringType, package ?? _packageContext);
        }

        /// <summary>
        /// Calls the page builder plug-in to return a fluent interface for configuring a page.
        /// You can pass a page instance or allow the builder to build one from scratch
        /// </summary>
        /// <param name="pageInstance">Optional instance to configure</param>
        /// <param name="declaringType">Used to configure the instance from custom attributes</param>
        /// <returns></returns>
        public IPageDefinition BuildUpPage(object pageInstance, Type declaringType, IPackage package)
        {
            if (PageBuilder == null)
                throw new FluentBuilderException("There is no build engine installed that knows how to build pages");
            return PageBuilder.BuildUpPage(pageInstance, declaringType, package ?? _packageContext);
        }

        /// <summary>
        /// Calls the service builder plug-in to return a fluent interface for configuring a service.
        /// You can pass a service instance or allow the builder to build one from scratch
        /// </summary>
        /// <param name="serviceInstance">Optional instance to configure</param>
        /// <param name="declaringType">Used to configure the instance from custom attributes</param>
        /// <returns></returns>
        public IServiceDefinition BuildUpService(object serviceInstance, Type declaringType, IPackage package)
        {
            if (ServiceBuilder == null)
                throw new FluentBuilderException("There is no build engine installed that knows how to build services");
            return ServiceBuilder.BuildUpService(serviceInstance, declaringType, package ?? _packageContext);
        }

        /// <summary>
        /// Calls the module builder plug-in to return a fluent interface for configuring a module.
        /// You can pass a module instance or allow the builder to build one from scratch
        /// </summary>
        /// <param name="moduleInstance">Optional instance to configure</param>
        /// <param name="declaringType">Used to configure the instance from custom attributes</param>
        /// <returns></returns>
        public IModuleDefinition BuildUpModule(object moduleInstance, Type declaringType)
        {
            if (ModuleBuilder == null)
                throw new FluentBuilderException("There is no build engine installed that knows how to build modules");
            return ModuleBuilder.BuildUpModule(moduleInstance, declaringType);
        }

        /// <summary>
        /// Calls the data provider builder plug-in to return a fluent interface for configuring a data provider.
        /// You can pass a data provider instance or allow the builder to build one from scratch
        /// </summary>
        /// <param name="dataProviderInstance">Optional instance to configure</param>
        /// <param name="declaringType">Used to configure the instance from custom attributes</param>
        /// <returns></returns>
        public IDataProviderDefinition BuildUpDataProvider(object dataProviderInstance, Type declaringType, IPackage package)
        {
            if (DataProviderBuilder == null)
                throw new FluentBuilderException("There is no build engine installed that knows how to build data providers");
            return DataProviderBuilder.BuildUpDataProvider(dataProviderInstance, declaringType, package ?? _packageContext);
        }

        /// <summary>
        /// Calls the module builder plug-in to return a fluent interface for configuring a module.
        /// You can pass a module instance or allow the builder to build one from scratch
        /// </summary>
        /// <param name="packageInstance">Optional instance to configure</param>
        /// <param name="declaringType">Used to configure the instance from custom attributes</param>
        /// <returns></returns>
        public IPackageDefinition BuildUpPackage(object packageInstance, Type declaringType)
        {
            if (PackageBuilder == null)
                throw new FluentBuilderException("There is no build engine installed that knows how to build packages");
            return PackageBuilder.BuildUpPackage(packageInstance, declaringType);
        }

        #endregion

        #region Builders for each element type

        /// <summary>
        /// This is called when a module is discovered through reflection and it needs
        /// to be consutucted and initialized, or when a package is explicitly registered
        /// with a custom namespace
        /// </summary>
        private IPackage BuildPackage(IPackage package, Type type, Func<Type, object> factory, string namespaceName)
        {
            if (package == null && factory != null && typeof(IPackage).IsAssignableFrom(type))
                package = factory(type) as IPackage;

            var packageDefinition = BuildUpPackage(package, type);

            package = packageDefinition.Build();

            if (!string.IsNullOrEmpty(namespaceName))
                package.NamespaceName = namespaceName;

            var packageBuilder = new FluentBuilder(
                this,
                package,
                _dataCatalog,
                _dataDependencyFactory,
                _dataSupplierFactory);

            package.Build(packageBuilder);
            
            return package;
        }

        /// <summary>
        /// This is called when a module is discovered through reflection and it needs
        /// to be consutucted and initialized
        /// </summary>
        private IModule BuildModule(Type type, Func<Type, object> factory)
        {
            IModule module = null;

            if (factory != null && typeof(IModule).IsAssignableFrom(type))
                module = factory(type) as IModule;

            var moduleDefinition = BuildUpModule(module, type);
            module = moduleDefinition.Build();

            return module;
        }

        /// <summary>
        /// This is called when a page is discovered through reflection and it needs
        /// to be consutucted and initialized
        /// </summary>
        private IPage BuildPage(Type type, Func<Type, object> factory)
        {
            IPage page = null;

            if (factory != null && typeof(IPage).IsAssignableFrom(type))
                page = factory(type) as IPage;

            var pageDefinition = BuildUpPage(page, type, _packageContext);
            page = pageDefinition.Build();

            return page;
        }

        /// <summary>
        /// This is called when a layout is discovered through reflection and it needs
        /// to be consutucted and initialized
        /// </summary>
        private ILayout BuildLayout(Type type, Func<Type, object> factory)
        {
            ILayout layout = null;

            if (factory != null && typeof(ILayout).IsAssignableFrom(type))
                layout = factory(type) as ILayout;

            var layoutDefinition = BuildUpLayout(layout, type, _packageContext);
            layout = layoutDefinition.Build();

            return layout;
        }

        /// <summary>
        /// This is called when a region is discovered through reflection and it needs
        /// to be consutucted and initialized
        /// </summary>
        private IRegion BuildRegion(Type type, Func<Type, object> factory)
        {
            IRegion region = null;

            if (factory != null && typeof(IRegion).IsAssignableFrom(type))
                region = factory(type) as IRegion;

            var regionDefinition = BuildUpRegion(region, type, _packageContext);
            region = regionDefinition.Build();

            return region;
        }

        /// <summary>
        /// This is called when a component is discovered through reflection and it needs
        /// to be consutucted and initialized
        /// </summary>
        private IComponent BuildComponent(Type type, Func<Type, object> factory)
        {
            IComponent component = null;

            if (factory != null && typeof(IComponent).IsAssignableFrom(type))
                component = factory(type) as IComponent;

            var componentDefinition = BuildUpComponent(component, type, _packageContext);
            component = componentDefinition.Build();

            return component;
        }

        /// <summary>
        /// This is called when a service is discovered through reflection and it needs
        /// to be consutucted and initialized
        /// </summary>
        private IService BuildService(Type type, Func<Type, object> factory)
        {
            IService service = null;

            if (factory != null && typeof(IService).IsAssignableFrom(type))
                service = factory(type) as IService;

            var serviceDefinition = BuildUpService(service, type, _packageContext);
            service = serviceDefinition.Build();

            return service;
        }

        private IDataProvider BuildDataProvider(Type type, Func<Type, object> factory)
        {
            IDataProvider dataProvider = null;

            if (factory != null && typeof(IDataProvider).IsAssignableFrom(type))
                dataProvider = factory(type) as IDataProvider;

            var dataProviderDefinition = BuildUpDataProvider(dataProvider, type, _packageContext);
            dataProvider = dataProviderDefinition.Build();

            return dataProvider;
        }

        #endregion

    }
}
