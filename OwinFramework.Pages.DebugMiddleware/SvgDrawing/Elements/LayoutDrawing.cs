using System.Collections.Generic;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Elements
{
    internal class LayoutDrawing : ElementDrawing
    {
        public LayoutDrawing(
                IDebugDrawing drawing, 
                DrawingElement page, 
                DebugLayout debugLayout,
                int headingLevel,
                bool showButtons)
            : base(
                page, 
                "Layout '" + debugLayout.Name + "'",
                headingLevel)
        {
            LeftMargin = 15f;
            RightMargin = 15f;
            TopMargin = 15f;
            BottomMargin = 15f;

            ChildSpacing = 15f;

            CssClass = "layout";

            var details = new List<string>();
            AddDebugInfo(details, debugLayout);

            if (details.Count > 0)
            {
                if (showButtons)
                    AddDetails(details, AddHeaderButton(page, "Detail"));
                else
                    AddDetails(details, this);
            }

            if (debugLayout.Regions != null)
            {
                foreach(var debugRegion in debugLayout.Regions)
                {
                    AddChild(new LayoutRegionDrawing(
                        drawing, 
                        page, 
                        debugRegion, 
                        headingLevel, 
                        showButtons));
                }
            }
        }
    }
}