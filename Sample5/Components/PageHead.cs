using OwinFramework.Pages.Core.Attributes;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Elements;

namespace Sample5.Components
{
    [IsComponent("page_head")]
    [PartOf("application_package")]
    [Description("Defines the contents of the title bar across the top of the website")]
    public class PageHead : Component
    {
        public PageHead(IComponentDependenciesFactory dependencies) : base(dependencies)
        {
            PageAreas = new[] { PageArea.Body };
        }

        public override IWriteResult WritePageArea(IRenderContext context, PageArea pageArea)
        {
            if (pageArea == PageArea.Body)
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