using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Elements
{
    internal class RouteDrawing: RectangleDrawing
    {
        public RouteDrawing(DebugRoute route)
        {
            CssClass = "list";

            var lines = new List<string>();

            if (route.Instance != null)
                lines.Add(route.Instance.GetType().DisplayName());

            lines.Add(route.ToString());

            AddChild(new TextDrawing { Text = lines.ToArray() });
        }

        protected override void ArrangeChildren()
        {
            ArrangeChildrenVertically(3);
        }
    }
}