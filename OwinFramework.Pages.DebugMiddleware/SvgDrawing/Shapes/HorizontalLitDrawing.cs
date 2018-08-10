using Svg;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes
{
    internal class HorizontalListDrawing : DrawingElement
    {
        public float ElementSpacing = 5f;

        protected override void ArrangeChildren()
        {
            ArrangeChildrenHorizontally(ElementSpacing);
        }
    }
}