using System.Collections.Generic;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Elements
{
    internal class LayoutRegionDrawing : ElementDrawing
    {
        public LayoutRegionDrawing(
                IDebugDrawing drawing, 
                DrawingElement page, 
                DebugLayoutRegion debugLayoutRegion,
                int headingLevel,
                bool showButtons)
            : base(
                page, 
                "'" + debugLayoutRegion.Name + "'",
                headingLevel)
        {
            LeftMargin = 3;
            RightMargin = 3;
            TopMargin = 3;
            BottomMargin = 3;

            CssClass = "layout-region";

            if (debugLayoutRegion.Region != null)
            {
                var region = new RegionDrawing(
                    drawing, 
                    page, 
                    debugLayoutRegion.Region, 
                    headingLevel, 
                    showButtons);
                AddChild(region);
            }

            var details = new List<string>();
            AddDebugInfo(details, debugLayoutRegion);

            if (details.Count > 0)
            {
                if (showButtons)
                    AddDetails(details, AddHeaderButton(page, "Detail"));
                else
                    AddDetails(details, this);
            }
        }
    }
}