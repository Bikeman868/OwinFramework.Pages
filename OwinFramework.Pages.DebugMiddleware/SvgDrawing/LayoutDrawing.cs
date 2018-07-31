using OwinFramework.Pages.Core.Debug;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing
{
    internal class LayoutDrawing: DrawingElement
    {
        public LayoutDrawing(DebugSvgDrawing drawing, DebugLayout debugLayout)
        {
            LeftMargin = 5;
            RightMargin = 5;
            TopMargin = 5;
            BottomMargin = 5;

            var layout = new RectangleDrawing { CssClass = "layout" };
            AddChild(layout);

            var text = new TextDrawing();
            text.Text.Add("Layout '" + debugLayout.Name + "'");
            text.CalculateSize();
            layout.AddChild(text);

            if (debugLayout.Regions != null)
            {
                var x = LeftMargin;
                foreach(var debugRegion in debugLayout.Regions)
                {
                    var region = new LayoutRegionDrawing(drawing, debugRegion);
                    region.Left = x;
                    region.Top = text.Top + text.Height + 8;
                    region.CalculateSize();
                    x += region.Width + 5;
                    layout.AddChild(region);
                }
            }
        }
    }
}