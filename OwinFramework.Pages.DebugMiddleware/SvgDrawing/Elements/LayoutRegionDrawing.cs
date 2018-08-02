using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Elements
{
    internal class LayoutRegionDrawing : DrawingElement
    {
        public LayoutRegionDrawing(IDebugDrawing drawing, DrawingElement page, DebugLayoutRegion debugLayoutRegion)
        {
            LeftMargin = 5;
            RightMargin = 5;
            TopMargin = 5;
            BottomMargin = 5;

            var layoutRegion = new DrawingElement { CssClass = "layout-region" };
            AddChild(layoutRegion);

            var text = new TextDrawing();
            text.Text.Add("'" + debugLayoutRegion.Name + "'");
            text.CalculateSize();
            layoutRegion.AddChild(text);

            if (debugLayoutRegion.Region != null)
            {
                var region = new RegionDrawing(drawing, page, debugLayoutRegion.Region);
                region.Top = text.Top + text.Height + 5;
                layoutRegion.AddChild(region);
            }
        }
    }
}