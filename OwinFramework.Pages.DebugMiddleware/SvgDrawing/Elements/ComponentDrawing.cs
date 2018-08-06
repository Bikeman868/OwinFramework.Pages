using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Elements
{
    internal class ComponentDrawing : RectangleDrawing
    {
        public ComponentDrawing(IDebugDrawing drawing, DrawingElement page, DebugComponent debugComponent)
        {
            CssClass = "component";

            AddChild(new TextDrawing
            {
                Text = new[] { "Component '" + debugComponent.Name + "'" }
            });
        }
    }
}