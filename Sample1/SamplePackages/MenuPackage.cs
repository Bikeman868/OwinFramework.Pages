using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Framework.Runtime;
using OwinFramework.Pages.Html.Runtime;

namespace Sample1.SamplePackages
{
    /// <summary>
    /// This demonstrates how to write a code only package. This package defines
    /// a layout called 'menu' that will display a list of 'MenuItem' objects
    /// from the data context.
    /// To use this package put the 'Menu' layout into a region and write a
    /// context handler to add a list of MenuPackage.MenuItem objects to the
    /// data context.
    /// Note that adding the [IsPackahe] attribute will make this package
    /// register automatically if you register the assembly that contains it, 
    /// which means that you can not override the namespace.
    /// If you want to override the namespace on a package that has the
    /// [IsPackage] attibute make sure that you register it manually before
    /// registering the assembly that contains it.
    /// </summary>
    [IsPackage("Menu", "menu")]
    public class MenuPackage : OwinFramework.Pages.Framework.Runtime.Package
    {
        public class MenuItem
        {
            public string Name { get; set; }
        }

        private class MenuItemComponent: Component
        {
            public override IWriteResult WriteHtml(IRenderContext renderContext, IDataContext dataContext)
            {
                var menuItem = dataContext.Get<MenuItem>();
                if (menuItem != null)
                    renderContext.Html.Write(menuItem.Name);
                return WriteResult.Continue();
            }
        }

        public override IPackage Build(IFluentBuilder builder)
        {
            var menuItemComponent = new MenuItemComponent();

            var menuBarRegion = builder.Region()
                .ForEach<MenuItem>()
                .Tag("li")
                .Style("display: inline-block;")
                .Component(menuItemComponent)
                .Build();

            builder.Layout()
                .Region("menu", menuBarRegion)
                .Tag("ul")
                .Build();

            return this;
        }

    }
}