using OwinFramework.Pages.CMS.Editor.Configuration;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Elements;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.CMS.Editor.Assets
{
    internal class AssetLinksComponent : Component
    {
        private readonly EditorConfiguration _configuraton;

        public AssetLinksComponent(
            EditorConfiguration configuraton,
            IComponentDependenciesFactory dependencies)
            : base(dependencies)
        {
            _configuraton = configuraton;
            PageAreas = new[] { PageArea.Head };
        }

        public override IWriteResult WritePageArea(IRenderContext context, PageArea pageArea)
        {
            if (pageArea == PageArea.Head)
            {
                context.Html.WriteElementLine("script", string.Empty, "src", _configuraton.ServiceBasePath + "assets/script");
            }

            return WriteResult.Continue();
        }
    }
}
