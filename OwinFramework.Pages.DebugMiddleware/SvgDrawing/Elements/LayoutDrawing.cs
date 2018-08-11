using System.Collections.Generic;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Elements
{
    internal class LayoutDrawing : ElementDrawing
    {
        private readonly List<DrawingElement> _layoutRegions = new List<DrawingElement>();

        public LayoutDrawing(IDebugDrawing drawing, DrawingElement page, DebugLayout debugLayout)
            : base(
            page, 
            "Layout '" + debugLayout.Name + "'",
            2,
            true,
            false)
        {
            CssClass = "layout";

            if (debugLayout.Regions != null)
            {
                foreach(var debugRegion in debugLayout.Regions)
                {
                    var layoutRegion = new LayoutRegionDrawing(drawing, page, debugRegion);
                    AddChild(layoutRegion);
                    _layoutRegions.Add(layoutRegion);
                }
            }

            if (ClassPopup != null)
            {
                var details = new List<string>();
                AddDebugInfo(details, debugLayout);
                AddDetails(details, ClassPopup);
            }
        }

        protected override void ArrangeChildren()
        {
            Header.Left = LeftMargin;
            Header.Top = TopMargin;
            Header.Arrange();

            var x = LeftMargin;
            var y = Header.Top + Header.Height + 8;

            foreach(var layoutRegion in _layoutRegions)
            {
                layoutRegion.Left = x;
                layoutRegion.Top = y;
                layoutRegion.Arrange();

                x += layoutRegion.Width + 5;
            }
        }
    }
}