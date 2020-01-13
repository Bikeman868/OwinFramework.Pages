using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Elements;

namespace Sample5.Components
{
    [IsComponent("resources")]
    [NeedsComponent("libraries:vue")]
    [PartOf("application_package")]
    [Description("Writes resources required on every page of the website")]
    public class Resources : Component
    {
        public Resources(IComponentDependenciesFactory dependencies) : base(dependencies)
        {
            PageAreas = new[] { PageArea.Head, PageArea.Body };
        }

        public override IWriteResult WritePageArea(IRenderContext context, PageArea pageArea)
        {
            if (pageArea == PageArea.Head)
            {
                context.Html.WriteElementLine("meta", null, "name", "viewport", "content", "width=device-width, initial-scale=1.0");
                context.Html.WriteElementLine("link", null, "rel", "stylesheet", "type", "text/css", "href", "/styles/main.css");
            }
            else if (pageArea == PageArea.Body)
            {
                context.Html.WriteElementLine("script", null, "src", "/scripts/main.js");
                context.Html.WriteElementLine("div", null, "id", "pageMask");
            }

            return base.WritePageArea(context, pageArea);
        }
    }
}