using System.Collections.Generic;
using System.Linq;
using Svg;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes
{
    internal class TitledListDrawing : TitledDrawing
    {
        public TitledListDrawing(string title, IEnumerable<string> list, int headingLevel = 3)
            : base(title, headingLevel)
        {
            CssClass = "list";
            AddChild(new TextDetailsDrawing { Text = list.ToArray() });
        }

        protected override void ArrangeChildren()
        {
            ArrangeChildrenVertically(3);
        }
    }
}