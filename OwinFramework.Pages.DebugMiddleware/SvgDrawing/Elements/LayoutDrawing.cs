using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes;
using Svg;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Elements
{
    internal class LayoutDrawing : RectangleDrawing
    {
        private readonly PopupBoxDrawing _popup;
        private readonly TextDrawing _title;

        public LayoutDrawing(IDebugDrawing drawing, DrawingElement page, DebugLayout debugLayout)
        {
            LeftMargin = 5;
            RightMargin = 5;
            TopMargin = 5;
            BottomMargin = 5;
            CssClass = "layout";

            _title = new TextDrawing
            {
                Left = LeftMargin,
                Top = TopMargin,
                Text = new[] { "Layout '" + debugLayout.Name + "'" }
            };
            AddChild(_title);

            if (debugLayout.Regions != null)
            {
                var x = LeftMargin;
                foreach(var debugRegion in debugLayout.Regions)
                {
                    var region = new LayoutRegionDrawing(drawing, page, debugRegion)
                    {
                        Left = x,
                        Top = _title.Top + _title.Height + 8
                    };
                    AddChild(region);

                    x += region.Width + 5;
                }
            }

            _popup = new PopupBoxDrawing();

            _popup.AddChild(new TextDrawing
            {
                Text = new[] { "Layout '" + debugLayout.Name + "'" }
            });

            page.AddChild(_popup);
        }

        public override void Arrange()
        {
            base.Arrange();

            float absoluteLeft, absoluteTop;
            GetAbsolutePosition(out absoluteLeft, out absoluteTop);

            _popup.SetAbsolutePosition(absoluteLeft, absoluteTop);
        }

        public override SvgElement Draw()
        {
            var drawing = base.Draw();
            _popup.Attach(_title);
            return drawing;
        }
    }
}