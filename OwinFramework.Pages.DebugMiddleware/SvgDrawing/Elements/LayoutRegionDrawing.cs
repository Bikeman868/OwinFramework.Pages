using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Elements
{
    internal class LayoutRegionDrawing : RectangleDrawing
    {
        public LayoutRegionDrawing(IDebugDrawing drawing, DrawingElement page, DebugLayoutRegion debugLayoutRegion)
        {
            CssClass = "layout-region";

            var title = new TextDrawing
            {
                Text = new[] { "'" + debugLayoutRegion.Name + "'" }
            };
            AddChild(title);

            if (debugLayoutRegion.Region != null)
            {
                var region = new RegionDrawing(drawing, page, debugLayoutRegion.Region);
                AddChild(region);
            }
        }

        protected override void ArrangeChildren()
        {
            ArrangeChildrenVertically(5);
        }
    }
}