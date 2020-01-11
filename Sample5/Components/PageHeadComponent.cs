using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Elements;

namespace Sample5.Components
{
    [IsComponent("page__head")]
    [PartOf("application_package")]
    [Description("Common header that is present on all pages")]
    public class PageHead : Component
    {
        public PageHead(IComponentDependenciesFactory dependencies) : base(dependencies)
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
                context.Html.WriteOpenTag("a", "class", Package.NamespaceName + "_page-head", "href", "/");
                context.Html.WriteLine();
                context.Html.WriteElementLine("h1", "Sample5");
                context.Html.WriteElementLine("p", "An example of how to create a website with the Owin Framework");
                context.Html.WriteCloseTag("a");
                context.Html.WriteLine();
            }

            return base.WritePageArea(context, pageArea);
        }
    }
}