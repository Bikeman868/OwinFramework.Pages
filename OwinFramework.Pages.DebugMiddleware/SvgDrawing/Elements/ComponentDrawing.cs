using System.Collections.Generic;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Elements
{
    internal class ComponentDrawing : ElementDrawing
    {
        public ComponentDrawing(
                IDebugDrawing drawing, 
                DrawingElement page, 
                DebugComponent debugComponent,
                int headingLevel,
                bool showButtons)
            : base(
                page, 
                "Component '" + debugComponent.Name + "'",
                headingLevel)
        {
            CssClass = "component";

            var details = new List<string>();
            AddDebugInfo(details, debugComponent);

            if (details.Count > 0)
            {
                if (showButtons)
                    AddDetails(details, AddHeaderButton(page, "Detail"));
                else
                    AddDetails(details, this);
            }
        }
    }
}