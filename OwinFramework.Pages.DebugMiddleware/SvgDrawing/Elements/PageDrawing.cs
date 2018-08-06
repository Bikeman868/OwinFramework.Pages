using System.Collections.Generic;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Elements
{
    internal class PageDrawing : RectangleDrawing
    {
        public PageDrawing(IDebugDrawing drawing, DebugPage debugPage)
        {
            LeftMargin = 20;
            RightMargin = 20;
            TopMargin = 20;
            BottomMargin = 20;
            CssClass = "page";

            var text = new List<string>();

            text.Add("Page '" + debugPage.Name + "'");

            if (debugPage.Routes != null)
            {
                foreach (var route in debugPage.Routes)
                    text.Add(route.Route);
            }

            var textDrawing = new TextDrawing
            {
                Text = text.ToArray()
            };
            AddChild(textDrawing);

            if (debugPage.Layout != null)
            {
                var layout = new LayoutDrawing(drawing, this, debugPage.Layout);
                AddChild(layout);
            }
        }

        protected override void ArrangeChildren()
        {
            ArrangeChildrenVertically(8);
        }
    }
}