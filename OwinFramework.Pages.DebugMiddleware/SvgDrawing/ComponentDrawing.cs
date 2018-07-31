using OwinFramework.Pages.Core.Debug;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing
{
    internal class ComponentDrawing : DrawingElement
    {
        public ComponentDrawing(DebugSvgDrawing drawing, DebugComponent debugComponent)
        {
            LeftMargin = 5;
            RightMargin = 5;
            TopMargin = 5;
            BottomMargin = 5;

            var layout = new RectangleDrawing { CssClass = "component" };
            AddChild(layout);

            var text = new TextDrawing();
            text.Text.Add("Component '" + debugComponent.Name + "'");
            text.CalculateSize();
            layout.AddChild(text);
        }
    }
}