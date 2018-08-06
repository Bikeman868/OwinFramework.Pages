using System.Collections.Generic;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Elements
{
    internal class LayoutRegionDrawing : ElementDrawing
    {
        public LayoutRegionDrawing(IDebugDrawing drawing, DrawingElement page, DebugLayoutRegion debugLayoutRegion)
            : base(page, "'" + debugLayoutRegion.Name + "'")
        {
            CssClass = "layout-region";

            if (debugLayoutRegion.Region != null)
            {
                var region = new RegionDrawing(drawing, page, debugLayoutRegion.Region);
                AddChild(region);
            }

            var details = new List<string>();
            AddDebugInfo(details, debugLayoutRegion);
            AddDetails(details, Popup);
        }

        protected override void ArrangeChildren()
        {
            ArrangeChildrenVertically(5);
        }
    }
}