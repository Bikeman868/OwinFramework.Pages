using System.Collections.Generic;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Elements
{
    internal class LayoutRegionDrawing : ElementDrawing
    {
        public LayoutRegionDrawing(IDebugDrawing drawing, DrawingElement page, DebugLayoutRegion debugLayoutRegion)
            : base(
            page, 
            "'" + debugLayoutRegion.Name + "'",
            2,
            false,
            false)
        {
            CssClass = "layout-region";

            if (debugLayoutRegion.Region != null)
            {
                var region = new RegionDrawing(drawing, page, debugLayoutRegion.Region);
                AddChild(region);
            }

            if (ClassPopup != null)
            {
                var details = new List<string>();
                AddDebugInfo(details, debugLayoutRegion);
                AddDetails(details, ClassPopup);
            }
        }

        protected override void ArrangeChildren()
        {
            ArrangeChildrenVertically(5);
        }
    }
}