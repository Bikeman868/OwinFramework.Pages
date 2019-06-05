using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;

namespace OwinFramework.Pages.Standard
{
    public class LayoutsPackage : Framework.Runtime.Package
    {
        public LayoutsPackage(IPackageDependenciesFactory dependencies)
            : base(dependencies)
        {
            Name = "layouts";
            NamespaceName = "layouts";
        }

        public override IPackage Build(IFluentBuilder builder)
        {
            // This "div" region outputs a <div> container
            var divRegion = builder.BuildUpRegion()
                .Name("div")
                .Tag("div")
                .Build();

            // This "null" region outputs no markup
            var nullRegion = builder.BuildUpRegion()
                .Name("null")
                .Tag("")
                .Build();

            // The "full_page" layout has a single region
            var fullPageLayout = builder.BuildUpLayout()
                .Name("full_page")
                .Tag("div")
                .ClassNames("{ns}_ly_full_page")
                .DeployCss("div.{ns}_ly_full_page", "height:auto; width:auto; overflow-x: hidden; overflow-y: auto;")
                .ZoneNesting("main")
                .Region("main", nullRegion)
                .Build();

            // The "col_2_left_fixed" layout has two columns where the left column
            // has a fixed with. Specify the width using your application CSS
            var twoColumnFixedLeftLayout = builder.BuildUpLayout()
                .Name("col_2_left_fixed")
                .Tag("div")
                .ClassNames("{ns}_ly_col_2_left_fixed")
                .DeployCss("div.{ns}_ly_col_2_left_fixed", "overflow: hidden;")
                .DeployCss("div.{ns}_ly_col_2_left_fixed > div.{ns}_rg_col_left", "overflow: hidden; float: left; width: 250px;")
                .DeployCss("div.{ns}_ly_col_2_left_fixed > div.{ns}_rg_col_right", "margin-left: 250px;")
                .ZoneNesting("left,right")
                .Region("left", 
                    builder.BuildUpRegion()
                    .Tag("div")
                    .ClassNames("{ns}_rg_col_left")
                    .Build())
                .Region("right",
                    builder.BuildUpRegion()
                    .Tag("div")
                    .ClassNames("{ns}_rg_col_right")
                    .Build())
                .Build();

            // The "col_2_right_fixed" layout has two columns where the right column
            // has a fixed with. Specify the width using your application CSS
            var twoColumnFixedRightLayout = builder.BuildUpLayout()
                .Name("col_2_right_fixed")
                .Tag("div")
                .ClassNames("{ns}_ly_col_2_right_fixed")
                .DeployCss("div.{ns}_ly_col_2_right_fixed", "overflow: hidden;")
                .DeployCss("div.{ns}_ly_col_2_right_fixed > div.{ns}_rg_col_left", "overflow: hidden; width: auto;")
                .DeployCss("div.{ns}_ly_col_2_right_fixed > div.{ns}_rg_col_right", "width: 250px; float: right;")
                .ZoneNesting("right,left")
                .Region("left",
                    builder.BuildUpRegion()
                    .Tag("div")
                    .ClassNames("{ns}_rg_col_left")
                    .Build())
                .Region("right",
                    builder.BuildUpRegion()
                    .Tag("div")
                    .ClassNames("{ns}_rg_col_right")
                    .Build())
                .Build();

            return this;
        }
    }
}
