using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Elements
{
    internal class RegionDrawing : RectangleDrawing
    {
        public RegionDrawing(IDebugDrawing drawing, DrawingElement page, DebugRegion debugRegion)
        {
            LeftMargin = 5;
            RightMargin = 5;
            TopMargin = 5;
            BottomMargin = 5;
            CssClass = "region";

            var text = new TextDrawing
            {
                Left = LeftMargin,
                Top = TopMargin,
                Text = new[] { "Region '" + debugRegion.Name + "'" }
            };
            AddChild(text);

            if (debugRegion.Content != null)
            {
                var content = drawing.DrawDebugInfo(page, debugRegion.Content);
                content.Left = LeftMargin;
                content.Top = text.Top + text.Height + 5;
                AddChild(content);
            }
        }
    }
}