using System.Collections.Generic;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Interfaces.Runtime;
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

            if (showButtons && !ReferenceEquals(debugLayout.Element, null))
            {
                var elementDebugInfo = debugLayout.Element.GetDebugInfo<DebugLayout>();
                if (elementDebugInfo != null)
                {
                    AddHeaderButton(page, "Definition")
                        .AddChild(new LayoutDrawing(
                            drawing,
                            page,
                            elementDebugInfo,
                            headingLevel + 1,
                            false));
                }
            }

            if (debugLayout.Children != null)
            {
                foreach(var debugRegion in debugLayout.Children)
                {
                    AddChild(new LayoutRegionDrawing(
                        drawing, 
                        page, 
                        debugRegion as DebugLayoutRegion, 
                        headingLevel, 
                        showButtons));
                }
            }
        }
    }
}