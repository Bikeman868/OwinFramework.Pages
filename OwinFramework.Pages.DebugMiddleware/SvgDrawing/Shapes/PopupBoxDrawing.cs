using System;
using OwinFramework.Builder;
using Svg;
using Svg.Transforms;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes
{
    internal class PopupBoxDrawing : DrawingElement
    {
        public float CornerRadius;
        private readonly string _popupId;

        public PopupBoxDrawing(DrawingElement page)
        {
            LeftMargin = 5;
            RightMargin = 5;
            TopMargin = 5;
            BottomMargin = 5;
            CornerRadius = 6f;
            ZOrder = 100;

            _popupId = Guid.NewGuid().ToShortString();

            page.AddChild(this);
        }

        public void Attach(SvgElement element)
        {
            element.CustomAttributes.Add("onmousemove", "ShowPopup(evt, '" + _popupId + "')");
            element.CustomAttributes.Add("onmouseout", "HidePopup(evt, '" + _popupId + "')");
        }

        protected override SvgElement GetContainer()
        {
            var container = new SvgGroup();
            container.Transforms.Add(new SvgTranslate(Left, Top));
            container.ID = _popupId;
            container.CustomAttributes.Add("visibility", "hidden");

            if (String.IsNullOrEmpty(CssClass))
                container.CustomAttributes.Add("class", "popup " + _popupId);
            else
                container.CustomAttributes.Add("class", CssClass + " popup " + _popupId);

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