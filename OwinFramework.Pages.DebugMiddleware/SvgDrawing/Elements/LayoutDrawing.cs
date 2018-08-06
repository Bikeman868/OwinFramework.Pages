using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes;
using Svg;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Elements
{
    internal class LayoutDrawing : RectangleDrawing
    {
        private readonly PopupBoxDrawing _popup;
        private readonly TextDrawing _title;
        private readonly List<DrawingElement> _layoutRegions = new List<DrawingElement>();

        public LayoutDrawing(IDebugDrawing drawing, DrawingElement page, DebugLayout debugLayout)
        {
            CssClass = "layout";

            _title = new TextDrawing
            {
                Text = new[] { "Layout '" + debugLayout.Name + "'" }
            };
            AddChild(_title);

            if (debugLayout.Regions != null)
            {
                foreach(var debugRegion in debugLayout.Regions)
                {
                    var layoutRegion = new LayoutRegionDrawing(drawing, page, debugRegion);
                    AddChild(layoutRegion);
                    _layoutRegions.Add(layoutRegion);
                }
            }

            _popup = new PopupBoxDrawing();

            _popup.AddChild(new TextDrawing
            {
                Text = new[] { "Layout '" + debugLayout.Name + "'" }
            });

            page.AddChild(_popup);
        }

        protected override void ArrangeChildren()
        {
            _title.Left = LeftMargin;
            _title.Top = TopMargin;
            _title.Arrange();

            var x = LeftMargin;
            var y = _title.Top + _title.Height + 8;

            foreach(var layoutRegion in _layoutRegions)
            {
                layoutRegion.Left = x;
                layoutRegion.Top = y;
                layoutRegion.Arrange();

                x += layoutRegion.Width + 5;
            }
        }

        public override void PositionPopups()
        {
            float absoluteLeft, absoluteTop;
            GetAbsolutePosition(out absoluteLeft, out absoluteTop);

            _popup.SetAbsolutePosition(absoluteLeft, absoluteTop);

            base.PositionPopups();
        }

        public override SvgElement Draw()
        {
            var drawing = base.Draw();
            _popup.Attach(_title);
            return drawing;
        }
    }
}