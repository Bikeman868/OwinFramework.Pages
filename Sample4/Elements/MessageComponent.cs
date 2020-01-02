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
    [IsComponent("message")]
    [Description("Displays a styled paragraph of text")]
    public class MessageComponent: Component
    {
        public string Message { get; set; }
        public string Style { get; set; }

        public MessageComponent(
            IComponentDependenciesFactory dependencies) 
            : base(dependencies)
        {
        }

        public override IWriteResult WritePageArea(IRenderContext context, PageArea pageArea)
        {
            if (pageArea == PageArea.Body)
            {
                context.Html.WriteComment("message component");
                context.Html.WriteUnclosedElement("hr");
                context.Html.WriteLine();
                context.Html.WriteElementLine("p", Message, "style", Style);
            }

            return WriteResult.Continue();
        }
    }
}