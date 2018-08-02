using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Elements
{
    internal class ComponentDrawing : DrawingElement
    {
        public ComponentDrawing(IDebugDrawing drawing, DrawingElement page, DebugComponent debugComponent)
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