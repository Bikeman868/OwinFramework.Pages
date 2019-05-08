using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Elements;
using OwinFramework.Pages.Html.Runtime;

namespace Sample4.Elements
{
    [IsComponent("test_widget")]
    public class SampleComponent: Component
    {
        public string Message { get; set; }

        public SampleComponent(
            IComponentDependenciesFactory dependencies) 
            : base(dependencies)
        {
        }

        public override IWriteResult WritePageArea(IRenderContext context, PageArea pageArea)
        {
            if (pageArea == PageArea.Body)
            {
                context.Html.WriteComment("Test widget");
                context.Html.WriteElementLine("p", Message);
            }

            return WriteResult.Continue();
        }
    }
}