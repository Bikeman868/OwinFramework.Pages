using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq.Modules;
using NUnit.Framework;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Managers;
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

            _nameManager = new NameManager();
        }

        [Test]
        public void Should_resolve_component_names()
        {
            var component = new Component(SetupMock<IComponentDependenciesFactory>());
            component.Name = "AbcDef";
            _nameManager.Register(component);

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

    }
}
