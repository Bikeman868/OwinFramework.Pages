using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Elements
{
    internal class RegionDrawing : RectangleDrawing
    {
        private readonly DrawingElement _title;
        private readonly DrawingElement _content;

        public RegionDrawing(IDebugDrawing drawing, DrawingElement page, DebugRegion debugRegion)
        {
            CssClass = "region";

            _title = new TextDrawing
            {
                Text = new[] { "Region '" + debugRegion.Name + "'" }
            };
            AddChild(_title);

            if (debugRegion.Content != null)
            {
                var content = drawing.DrawDebugInfo(page, debugRegion.Content);
                AddChild(content);
            }
        }

        protected override void ArrangeChildren()
        {
            ArrangeChildrenVertically(5);
        }
    }
}