using System.Collections.Generic;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Elements
{
    internal class ComponentDrawing : ElementDrawing
    {
        public ComponentDrawing(IDebugDrawing drawing, DrawingElement page, DebugComponent debugComponent)
            : base(page, "Component '" + debugComponent.Name + "'")
        {
            CssClass = "component";

            var details = new List<string>();
            AddDebugInfo(details, debugComponent);
            AddDetails(details, Popup);
        }
    }
}