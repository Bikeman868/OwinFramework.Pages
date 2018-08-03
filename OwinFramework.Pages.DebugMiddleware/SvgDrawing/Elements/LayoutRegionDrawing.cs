using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Elements
{
    internal class LayoutRegionDrawing : RectangleDrawing
    {
        public LayoutRegionDrawing(IDebugDrawing drawing, DrawingElement page, DebugLayoutRegion debugLayoutRegion)
        {
            LeftMargin = 5;
            RightMargin = 5;
            TopMargin = 5;
            BottomMargin = 5;
            CssClass = "layout-region";

            var text = new TextDrawing
            {
                Left = LeftMargin,
                Top = TopMargin,
                Text = new[] { "'" + debugLayoutRegion.Name + "'" }
            };
            AddChild(text);

            if (debugLayoutRegion.Region != null)
            {
                var region = new RegionDrawing(drawing, page, debugLayoutRegion.Region)
                {
                    Top = text.Top + text.Height + 5
                };
                AddChild(region);
            }
        }
    }
}