using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Html.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sample1.SamplePackages
{
    [IsPackage("regions")]
    public class LayoutExamples
    {
        [IsLayout("layout1", "zone1,zone2")]
        [Container("div")]
        private class LayoutExample1
        {}

        [IsLayout("layout2", "zone1,zone2")]
        [Container("div")]
        private class LayoutExample2 : Layout
        {
            public LayoutExample2(
                ILayoutDependenciesFactory layoutDependencies,
                IRegionDependenciesFactory regionDependencies)
                : base(layoutDependencies)
            {
                var region1 = new Region1(regionDependencies);
                var region2 = new Region2(regionDependencies);

                AddZone("zone1", region1);
                AddZone("zone2", region2);

                AddVisualElement(w => w.WriteOpenTag("div"), "Start of layout 2");
                AddZoneVisualElement("zone1");
                AddVisualElement(w => w.WriteOpenTag("pre"), "Start of zone 2 in layout 2");
                AddZoneVisualElement("zone2");
                AddVisualElement(w => w.WriteCloseTag("pre"), "End of zone 2 in layout 2");
                AddVisualElement(w => w.WriteCloseTag("div"), "End of layout 2");
            }

            private class Region1 : Region
            {
                public Region1(IRegionDependenciesFactory dependencies)
                    : base(dependencies)
                {
                }
            }

            private class Region2 : Region
            {
                public Region2(IRegionDependenciesFactory dependencies)
                    : base(dependencies)
                {
                }
            }
        }
    }
}