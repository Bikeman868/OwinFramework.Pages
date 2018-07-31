using OwinFramework.Pages.Core.Debug;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing
{
    internal class LayoutRegionDrawing : DrawingElement
    {
        public LayoutRegionDrawing(DebugSvgDrawing drawing, DebugLayoutRegion debugLayoutRegion)
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
                var region = new RegionDrawing(drawing, debugLayoutRegion.Region);
                region.Top = text.Top + text.Height + 5;
                layoutRegion.AddChild(region);
            }
        }
    }
}