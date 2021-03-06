﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using OwinFramework.Pages.Framework.Interfaces;

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
        private readonly Func<Type, object> _factory;

        private bool _debugLogging;

        public FluentBuilder(
            INameManager nameManager,
            IDataCatalog dataCatalog,
            IDataDependencyFactory dataDependencyFactory,
            IDataSupplierFactory dataSupplierFactory,
            IFrameworkConfiguration frameworkConfiguration)
        {
            _nameManager = nameManager;
            _dataCatalog = dataCatalog;
            _dataDependencyFactory = dataDependencyFactory;
            _dataSupplierFactory = dataSupplierFactory;
            _assemblies = new HashSet<string>();
            _types = new HashSet<Type>();

            frameworkConfiguration.Subscribe(config => _debugLogging = config.DebugLogging);
        }

        private FluentBuilder(
            FluentBuilder parent,
            Func<Type, object> factory,
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
            _factory = factory;

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

        IPackage IFluentBuilder.RegisterPackage(
            IPackage package, 
            string namespaceName, 
            Func<Type, object> factory)
        {
            var type = package.GetType();
            if (!_types.Add(type)) return package;

            if (_debugLogging) Trace.WriteLine(
                "Fluent builder registering package " + 
                type.FullName + 
                (string.IsNullOrEmpty(namespaceName) ? " with its default namespace" : " with namespace " + namespaceName));

            return BuildPackage(package, type, factory, namespaceName ?? package.NamespaceName);
        }

        IPackage IFluentBuilder.RegisterPackage(
            IPackage package, 
            Func<Type, object> factory)
        {
            var type = package.GetType();
            if (!_types.Add(type)) return package;

            if (_debugLogging) Trace.WriteLine("Fluent builder registering package " + type.FullName + " with its default namespace");

            return BuildPackage(package, type, factory, package.NamespaceName);
        }

        #endregion

        #region Enumerating types in an assembly

        void IFluentBuilder.Register(Assembly assembly, Func<Type, object> factory)
        {
            if (!_assemblies.Add(assembly.FullName))
                return;

            if (_debugLogging) Trace.WriteLine("Fluent builder is discovering elements in assembly " + assembly.FullName);

            var types = assembly.GetTypes().Where(t => t.IsClass && !t.ContainsGenericParameters && !t.IsInterface);
            var  exceptions = new List<Exception>();

            foreach (var type in types)
            {
                try
                {
                    var element = ((IFluentBuilder)this).Register(type, factory);
                    if (_debugLogging && element != null)
                    {
                        Trace.WriteLine("Fluent builder discovered and registered " + type.DisplayName() + " in asembly " + assembly.FullName);
                    }
                }
                catch (Exception ex)
                {
                    if (_debugLogging) Trace.WriteLine("EXCEPTION: Fluent builder caught exception " + 
                        ex.Message + "registering " + type.FullName + " from assembly " + assembly.FullName);
                    exceptions.Add(ex);
                }
            }

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

            // Note that any element can also be a service
            if (attributes.IsService != null)
            {
                if (_debugLogging) Trace.WriteLine("Fluent builder is registering a service of type " + type.FullName);
                BuildService(type, factory);
            }

            if (attributes.IsPackage != null)
            {
                if (_debugLogging) Trace.WriteLine("Fluent builder is registering a package of type " + type.FullName);
                return BuildPackage(null, type, factory, null);
            }
            if (attributes.IsModule != null)
            {
                if (_debugLogging) Trace.WriteLine("Fluent builder is registering a module of type " + type.FullName);
                return BuildModule(type, factory);
            }
            if (attributes.IsPage != null)
            {
                if (_debugLogging) Trace.WriteLine("Fluent builder is registering a page of type " + type.FullName);
                return BuildPage(type, factory);
            }
            if (attributes.IsLayout != null)
            {
                if (_debugLogging) Trace.WriteLine("Fluent builder is registering a layout of type " + type.FullName);
                return BuildLayout(type, factory);
            }
            if (attributes.IsRegion != null)
            {
                if (_debugLogging) Trace.WriteLine("Fluent builder is registering a region of type " + type.FullName);
                return BuildRegion(type, factory);
            }
            if (attributes.IsComponent != null)
            {
                if (_debugLogging) Trace.WriteLine("Fluent builder is registering a component of type " + type.FullName);
                return BuildComponent(type, factory);
            }
            if (attributes.IsDataProvider != null)
            {
                if (_debugLogging) Trace.WriteLine("Fluent builder is registering a data provider of type " + type.FullName);
                return BuildDataProvider(type, factory);
            }

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
            {
                if (_debugLogging) Trace.WriteLine("Fluent builder is registering the '" + 
                    package.Name + "' package of type " + obj.GetType().DisplayName() + " with the name manager");
                _nameManager.Register(package);
            }

            var module = obj as IModule;
            if (module != null)
            {
                if (_debugLogging) Trace.WriteLine("Fluent builder is registering the '" + 
                    module.Name + "' module of type " + obj.GetType().DisplayName() + " with the name manager");
                _nameManager.Register(module);
            }

            var runable = obj as IRunable;
            if (runable != null)
            {
                if (_debugLogging) Trace.WriteLine("Fluent builder is registering the '" + 
                    runable.Name + "' runable of type " + obj.GetType().DisplayName() + " with the name manager");
                _nameManager.Register(runable);
            }

            var element = obj as IElement;
            if (element != null)
            {
                if (_debugLogging) Trace.WriteLine("Fluent builder is registering the '" + 
                    element.Name + "' " + element.ElementType + " of type " + obj.GetType().DisplayName() + " with the name manager");
                _nameManager.Register(element);
            }

            var dataProvider = obj as IDataProvider;
            if (dataProvider != null)
            {
                if (_debugLogging) Trace.WriteLine("Fluent builder is registering the '" + 
                    dataProvider.Name + "' data provider of type " + obj.GetType().DisplayName() + " with the name manager");
                _nameManager.Register(dataProvider);
            }

            var dataSupplier = obj as IDataSupplier;
            if (dataSupplier != null)
            {
                if (_debugLogging) Trace.WriteLine("Fluent builder is registering data supplier of type " + 
                    obj.GetType().DisplayName() + " with the data catalog");
                _dataCatalog.Register(dataSupplier);
            }
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
        /// <param name="package">The namespace to use for the page assets</param>
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
        /// <param name="factory">Used to construct the service if the service uses constructor injection</param>
        /// <param name="package">The namespace to use for the service assets</param>
        /// <returns></returns>
        public IServiceDefinition BuildUpService(object serviceInstance, Type declaringType, Func<Type, object> factory, IPackage package)
        {
            if (ServiceBuilder == null)
                throw new FluentBuilderException("There is no build engine installed that knows how to build services");
            return ServiceBuilder.BuildUpService(serviceInstance, declaringType, factory ?? _factory, package ?? _packageContext);
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
        /// Calls the package builder plug-in to return a fluent interface for configuring a package.
        /// You can pass a package instance or allow the builder to build one from scratch
        /// </summary>
        /// <param name="packageInstance">Optional instance to configure</param>
        /// <param name="declaringType">Used to configure the instance from custom attributes</param>
        /// <param name="factory">If the package registers services then it needs a factory to construct them</param>
        /// <returns></returns>
        public IPackageDefinition BuildUpPackage(object packageInstance, Type declaringType, Func<Type, object> factory)
        {
            if (PackageBuilder == null)
                throw new FluentBuilderException("There is no build engine installed that knows how to build packages");
            return PackageBuilder.BuildUpPackage(packageInstance, declaringType, factory);
        }

        #endregion

        #region Builders for each element type

        /// <summary>
        /// This is called when a package is discovered through reflection and it needs
        /// to be consutucted and initialized, or when a package is explicitly registered
        /// with a custom namespace
        /// </summary>
        private IPackage BuildPackage(IPackage package, Type type, Func<Type, object> factory, string namespaceName)
        {
            if (_debugLogging)
            {
                var message = "Fluent builder is building";

                if (package != null)
                {
                    message += " an instance of package " + package.GetType().DisplayName();
                    if (type != null) message += " using attributes from " + type.DisplayName();
                }
                else
                {
                    if (type == null)
                        message += " a package of unspecified type";
                    else
                        message += " a package of type " + type.DisplayName();

                    if (factory == null)
                        message += " using the default public constructor";
                    else
                        message += " using a factory";
                }

                if (!string.IsNullOrEmpty(namespaceName))
                    message += " defining the '" + namespaceName + "' namespace";

                Trace.WriteLine(message);
            }

            if (package == null && factory != null && typeof(IPackage).IsAssignableFrom(type))
                package = factory(type) as IPackage;

            var packageDefinition = BuildUpPackage(package, type, factory);

            package = packageDefinition.Build();

            if (!string.IsNullOrEmpty(namespaceName))
                package.NamespaceName = namespaceName;

            var packageBuilder = new FluentBuilder(
                this,
                factory,
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
            if (_debugLogging)
            {
                var message = "Fluent builder is building";

                if (type == null)
                    message += " a module of unknown type";
                else
                    message += " a module of type " + type.DisplayName();

                if (factory == null)
                    message += " using the default public constructor";
                else
                    message += " using a factory";

                Trace.WriteLine(message);
            }

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
            if (_debugLogging)
            {
                var message = "Fluent builder is building";

                if (type == null)
                    message += " a page of unknown type";
                else
                    message += " a page of type " + type.DisplayName();

                if (factory == null)
                    message += " using the default public constructor";
                else
                    message += " using a factory";

                Trace.WriteLine(message);
            }

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
            if (_debugLogging)
            {
                var message = "Fluent builder is building";

                if (type == null)
                    message += " a layout of unknown type";
                else
                    message += " a layout of type " + type.DisplayName();

                if (factory == null)
                    message += " using the default public constructor";
                else
                    message += " using a factory";

                Trace.WriteLine(message);
            }

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
            if (_debugLogging)
            {
                var message = "Fluent builder is building";

                if (type == null)
                    message += " a region of unknown type";
                else
                    message += " a region of type " + type.DisplayName();

                if (factory == null)
                    message += " using the default public constructor";
                else
                    message += " using a factory";

                Trace.WriteLine(message);
            }

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
            if (_debugLogging)
            {
                var message = "Fluent builder is building";

                if (type == null)
                    message += " a component of unknown type";
                else
                    message += " a component of type " + type.DisplayName();

                if (factory == null)
                    message += " using the default public constructor";
                else
                    message += " using a factory";

                Trace.WriteLine(message);
            }

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
            if (_debugLogging)
            {
                var message = "Fluent builder is building";

                if (type == null)
                    message += " a service of unknown type";
                else
                    message += " a service of type " + type.DisplayName();

                if (factory == null)
                    message += " using the default public constructor";
                else
                    message += " using a factory";

                Trace.WriteLine(message);
            }

            IService service = null;

            if (factory != null && typeof(IService).IsAssignableFrom(type))
                service = factory(type) as IService;

            var serviceDefinition = BuildUpService(service, type, factory, _packageContext);
            service = serviceDefinition.Build();

            return service;
        }

        private IDataProvider BuildDataProvider(Type type, Func<Type, object> factory)
        {
            if (_debugLogging)
            {
                var message = "Fluent builder is building";

                if (type == null)
                    message += " a data provider of unknown type";
                else
                    message += " a data provider of type " + type.DisplayName();

                if (factory == null)
                    message += " using the default public constructor";
                else
                    message += " using a factory";

                Trace.WriteLine(message);
            }

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
