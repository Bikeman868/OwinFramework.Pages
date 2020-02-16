using System;
using Moq.Modules;
using NUnit.Framework;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Framework.Managers;
using OwinFramework.Pages.Html.Elements;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.UnitTests.Framework.Managers
{
    [TestFixture]
    public class NameManagerTests: TestBase
    {
        private INameManager _nameManager;

        [SetUp]
        public void Setup()
        {
            Reset();

            _nameManager = new NameManager(
                SetupMock<IFrameworkConfiguration>());
        }

        [Test]
        public void Should_resolve_component_names()
        {
            var component = new Component(SetupMock<IComponentDependenciesFactory>());
            component.Name = "AbcDef";
            _nameManager.Register(component);
            _nameManager.Bind();

            Assert.AreEqual(component, _nameManager.ResolveComponent("AbcDef"));
            Assert.AreEqual(component, _nameManager.ResolveComponent("abcdef"));
            Assert.AreEqual(component, _nameManager.ResolveComponent("ABCDEF"));

            Assert.Throws<NameResolutionFailureException>(() => _nameManager.ResolveComponent("Xyz"));

            Assert.Throws<NameResolutionFailureException>(() => _nameManager.ResolveDataProvider("AbcDef"));
            Assert.Throws<NameResolutionFailureException>(() => _nameManager.ResolveLayout("AbcDef"));
            Assert.Throws<NameResolutionFailureException>(() => _nameManager.ResolveModule("AbcDef"));
            Assert.Throws<NameResolutionFailureException>(() => _nameManager.ResolvePackage("AbcDef"));
            Assert.Throws<NameResolutionFailureException>(() => _nameManager.ResolvePage("AbcDef"));
            Assert.Throws<NameResolutionFailureException>(() => _nameManager.ResolveRegion("AbcDef"));
            Assert.Throws<NameResolutionFailureException>(() => _nameManager.ResolveService("AbcDef"));
        }

        [Test]
        public void Should_resolve_layout_names()
        {
            var layout = new Layout(SetupMock<ILayoutDependenciesFactory>());
            layout.Name = "AbcDef";
            _nameManager.Register(layout);
            _nameManager.Bind();

            Assert.AreEqual(layout, _nameManager.ResolveLayout("AbcDef"));
            Assert.AreEqual(layout, _nameManager.ResolveLayout("abcdef"));
            Assert.AreEqual(layout, _nameManager.ResolveLayout("ABCDEF"));

            Assert.Throws<NameResolutionFailureException>(() => _nameManager.ResolveLayout("Xyz"));

            Assert.Throws<NameResolutionFailureException>(() => _nameManager.ResolveComponent("AbcDef"));
            Assert.Throws<NameResolutionFailureException>(() => _nameManager.ResolveDataProvider("AbcDef"));
            Assert.Throws<NameResolutionFailureException>(() => _nameManager.ResolveModule("AbcDef"));
            Assert.Throws<NameResolutionFailureException>(() => _nameManager.ResolvePackage("AbcDef"));
            Assert.Throws<NameResolutionFailureException>(() => _nameManager.ResolvePage("AbcDef"));
            Assert.Throws<NameResolutionFailureException>(() => _nameManager.ResolveRegion("AbcDef"));
            Assert.Throws<NameResolutionFailureException>(() => _nameManager.ResolveService("AbcDef"));
        }

        [Test]
        public void Should_resolve_module_names()
        {
            var module = new Module(SetupMock<IModuleDependenciesFactory>());
            module.Name = "AbcDef";
            _nameManager.Register(module);
            _nameManager.Bind();

            Assert.AreEqual(module, _nameManager.ResolveModule("AbcDef"));
            Assert.AreEqual(module, _nameManager.ResolveModule("abcdef"));
            Assert.AreEqual(module, _nameManager.ResolveModule("ABCDEF"));

            Assert.Throws<NameResolutionFailureException>(() => _nameManager.ResolveModule("Xyz"));

            Assert.Throws<NameResolutionFailureException>(() => _nameManager.ResolveComponent("AbcDef"));
            Assert.Throws<NameResolutionFailureException>(() => _nameManager.ResolveDataProvider("AbcDef"));
            Assert.Throws<NameResolutionFailureException>(() => _nameManager.ResolveLayout("AbcDef"));
            Assert.Throws<NameResolutionFailureException>(() => _nameManager.ResolvePackage("AbcDef"));
            Assert.Throws<NameResolutionFailureException>(() => _nameManager.ResolvePage("AbcDef"));
            Assert.Throws<NameResolutionFailureException>(() => _nameManager.ResolveRegion("AbcDef"));
            Assert.Throws<NameResolutionFailureException>(() => _nameManager.ResolveService("AbcDef"));
        }

        [Test]
        public void Should_run_phased_resolution_actions()
        {
            var handlerCount = 0;
            var currentPhase = NameResolutionPhase.ResolvePackageNames;
            Action<INameManager, NameResolutionPhase> expectPhase = (nm, p) =>
                {
                    Assert.IsTrue(p >= currentPhase);
                    currentPhase = p;
                    handlerCount++;
                };

            var r = new Random();
            for (var i = 0; i < 1000; i++)
            {
                var phase = (NameResolutionPhase)(r.Next((int)NameResolutionPhase.ResolvePackageNames, (int)NameResolutionPhase.InitializeRunables + 1));
                _nameManager.AddResolutionHandler(phase, expectPhase, phase);
            }

            _nameManager.Bind();

            Assert.AreEqual(1000, handlerCount);
        }

        [Test]
        public void Should_separete_package_namespaces()
        {
            var package1 = new TestPackage { NamespaceName = "package1" };
            var package2 = new TestPackage { NamespaceName = "package2" };

            var layout1 = new Layout(SetupMock<ILayoutDependenciesFactory>());
            layout1.Name = "layout";
            layout1.Package = package1;
            _nameManager.Register(layout1);

            var layout2 = new Layout(SetupMock<ILayoutDependenciesFactory>());
            layout2.Name = "layout";
            layout2.Package = package2;
            _nameManager.Register(layout2);

            _nameManager.Bind();

            Assert.AreEqual(layout1, _nameManager.ResolveLayout("layout", package1));
            Assert.AreEqual(layout2, _nameManager.ResolveLayout("layout", package2));

            Assert.AreEqual(layout1, _nameManager.ResolveLayout("package1:layout"));
            Assert.AreEqual(layout1, _nameManager.ResolveLayout("package1:layout", package1));
            Assert.AreEqual(layout1, _nameManager.ResolveLayout("package1:layout", package2));

            Assert.AreEqual(layout2, _nameManager.ResolveLayout("package2:layout"));
            Assert.AreEqual(layout2, _nameManager.ResolveLayout("package2:layout", package1));
            Assert.AreEqual(layout2, _nameManager.ResolveLayout("package2:layout", package2));
        }

        [Test]
        public void Should_separete_package_namespaces_with_defered_package_resolution()
        {
            var package1 = new TestPackage { NamespaceName = "package1" };
            var package2 = new TestPackage { NamespaceName = "package2" };

            var layout1 = new Layout(SetupMock<ILayoutDependenciesFactory>());
            layout1.Name = "layout";
            _nameManager.Register(layout1);

            var layout2 = new Layout(SetupMock<ILayoutDependenciesFactory>());
            layout2.Name = "layout";
            _nameManager.Register(layout2);

            _nameManager.AddResolutionHandler(NameResolutionPhase.ResolvePackageNames, nm =>
                {
                    layout1.Package = package1;
                    layout2.Package = package2;
                });

            _nameManager.Bind();

            Assert.AreEqual(layout1, _nameManager.ResolveLayout("layout", package1));
            Assert.AreEqual(layout2, _nameManager.ResolveLayout("layout", package2));

            Assert.AreEqual(layout1, _nameManager.ResolveLayout("package1:layout"));
            Assert.AreEqual(layout1, _nameManager.ResolveLayout("package1:layout", package1));
            Assert.AreEqual(layout1, _nameManager.ResolveLayout("package1:layout", package2));

            Assert.AreEqual(layout2, _nameManager.ResolveLayout("package2:layout"));
            Assert.AreEqual(layout2, _nameManager.ResolveLayout("package2:layout", package1));
            Assert.AreEqual(layout2, _nameManager.ResolveLayout("package2:layout", package2));
        }

        private class TestPackage : IPackage
        {
            public ElementType ElementType { get { return ElementType.Package; } }
            public string Name { get; set; }
            public string NamespaceName { get; set; }
            public IModule Module { get; set; }

            public IPackage Build(IFluentBuilder fluentBuilder)
            {
                return this;
            }
        }

    }
}
