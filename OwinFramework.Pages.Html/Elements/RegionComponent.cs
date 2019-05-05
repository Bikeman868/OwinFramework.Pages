using System;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Core.Interfaces;

namespace OwinFramework.Pages.Html.Elements
{
    /// <summary>
    /// A lightweight version of the region class that provides a simple way to put a component
    /// directly into a layout without explicitly configuring a region element.
    /// </summary>
    public class RegionComponent : Region
    {
        public RegionComponent(IRegionDependenciesFactory dependencies)
            : base(dependencies)
        {
        }

        public override IWriteResult WritePageArea(
            IRenderContext context, 
            PageArea pageArea,
            Action<IRenderContext, object> onListItem,
            Func<IRenderContext, PageArea, IWriteResult> contentWriter)
        {
#if TRACE
            context.Trace(() => ToString() + " writing page " + Enum.GetName(typeof(PageArea), pageArea).ToLower());
#endif
            if (context.IncludeComments)
                context.Html.WriteComment("region component " + this.FullyQualifiedName());

            return contentWriter(context, pageArea);
        }
    }
}
