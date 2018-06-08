﻿using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.BaseClasses;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Facilities.Runtime;

namespace Sample1.Pages
{
    [IsPackage("Menu", "menu")]
    public class MenuPackage : OwinFramework.Pages.Facilities.Runtime.Package
    {
        public class MenuItem
        {
            public string Name { get; set; }
        }

        public override void Build(IFluentBuilder builder)
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
        }

        private class MenuItemComponent: Component
        {

        }
    }
}