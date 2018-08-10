using Svg;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes
{
    internal class VerticalListDrawing : DrawingElement
    {
        public float ElementSpacing = 5f;

        protected override void ArrangeChildren()
        {
            ArrangeChildrenVertically(ElementSpacing);
        }
    }
}