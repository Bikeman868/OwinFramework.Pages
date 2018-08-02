using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes;
using Svg;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Elements
{
    internal class LayoutDrawing: DrawingElement
    {
        private readonly PopupBoxDrawing _popup;

        public LayoutDrawing(IDebugDrawing drawing, DrawingElement page, DebugLayout debugLayout)
        {
            LeftMargin = 5;
            RightMargin = 5;
            TopMargin = 5;
            BottomMargin = 5;

            var layout = new RectangleDrawing { CssClass = "layout" };
            AddChild(layout);

            var text = new TextDrawing();
            text.Text.Add("Layout '" + debugLayout.Name + "'");
            text.CalculateSize();
            layout.AddChild(text);

            if (debugLayout.Regions != null)
            {
                var x = LeftMargin;
                foreach(var debugRegion in debugLayout.Regions)
                {
                    var region = new LayoutRegionDrawing(drawing, page, debugRegion);
                    region.Left = x;
                    region.Top = text.Top + text.Height + 8;
                    region.CalculateSize();
                    x += region.Width + 5;
                    layout.AddChild(region);
                }
            }


            var popupText = new TextDrawing();
            popupText.Text.Add("Layout '" + debugLayout.Name + "'");
            popupText.CalculateSize();

            _popup = new PopupBoxDrawing(page);
            _popup.AddChild(popupText);
        }

        protected override SvgElement GetContainer()
        {
            var container = base.GetContainer();
            _popup.Attach(container);
            return container;
        }
    }
}