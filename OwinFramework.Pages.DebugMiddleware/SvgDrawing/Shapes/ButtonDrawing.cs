using Svg;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes
{
    internal class ButtonDrawing : RectangleDrawing
    {
        public ButtonDrawing()
        {
            CssClass = "button";
            LeftMargin = 4f;
            TopMargin = 3f;
            BottomMargin = 3f;
            RightMargin = 2f;
            CornerRadius = 0.5f;
        }
    }
}