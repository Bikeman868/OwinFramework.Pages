using System.Collections.Generic;
using System.Linq;
using Svg;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes
{
    internal class TitledDrawing : RectangleDrawing
    {
        public TitledDrawing(string title, int headingLevel = 3)
        {
            if (!string.IsNullOrEmpty(title))
            {
                AddChild(
                    new TextDrawing { Text = new[] { title } }
                    .HeadingLevel(headingLevel));
            }
        }

        protected override void ArrangeChildren()
        {
            ArrangeChildrenVertically(3);
        }
    }
}