using Svg;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing
{
    internal class RectangleDrawing : DrawingElement
    {
        public float CornerRadius;

        public RectangleDrawing()
        {
            LeftMargin = 5;
            RightMargin = 5;
            TopMargin = 5;
            BottomMargin = 5;
            CornerRadius = 3f;
        }

        protected override SvgElement GetContainer(SvgDocument document)
        {
            var container = base.GetContainer(document);

            var rectangle = new SvgRectangle
            {
                Height = Height,
                Width = Width,
                CornerRadiusX = CornerRadius,
                CornerRadiusY = CornerRadius
            };
            container.Children.Add(rectangle);

            return container;
        }
    }
}