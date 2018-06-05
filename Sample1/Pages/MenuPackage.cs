using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.BaseClasses;
using OwinFramework.Pages.Core.Interfaces.Builder;

namespace Sample1.Pages
{
    [IsPackage("Menu", "menu")]
    public class MenuPackage : OwinFramework.Pages.Core.BaseClasses.Package
    {
        public override void Build(IFluentBuilder builder)
        {
            var menuBarComponent = new MenuBarComponent();

            var menuBarRegion = builder.Region()
                .Name("menu-bar")
                .Build();

            builder.Layout()
                .Name("menu-bar")
                .Region(menuBarRegion, "menu")
                .Component("menu", menuBarComponent)
                .Build();
        }

        private class MenuBarComponent: Component
        {

        }
    }
}