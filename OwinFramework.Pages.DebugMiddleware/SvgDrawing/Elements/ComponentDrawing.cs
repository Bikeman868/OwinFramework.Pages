using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Elements
{
    internal class ComponentDrawing : RectangleDrawing
    {
        public ComponentDrawing(IDebugDrawing drawing, DrawingElement page, DebugComponent debugComponent)
        {
            LeftMargin = 5;
            RightMargin = 5;
            TopMargin = 5;
            BottomMargin = 5;
            CssClass = "component";

            AddChild(new TextDrawing
            {
                Left = LeftMargin,
                Top = TopMargin,
                Text = new[] { "Component '" + debugComponent.Name + "'" }
            });
        }
    }
}