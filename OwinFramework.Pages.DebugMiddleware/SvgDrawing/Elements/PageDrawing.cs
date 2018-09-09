using System.Collections.Generic;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Elements
{
    internal class PageDrawing : ElementDrawing
    {
        public PageDrawing(
                IDebugDrawing drawing, 
                DebugPage debugPage,
                int headingLevel,
                bool showButtons)
            : base(
                null, 
                "Page '" + debugPage.Name + "'",
                headingLevel)
        {
            LeftMargin = 20;
            RightMargin = 20;
            TopMargin = 20;
            BottomMargin = 20;

            CssClass = "page";

            var text = new List<string>();

            if (!string.IsNullOrEmpty(debugPage.RequiredPermission))
                text.Add("Requires the '" + debugPage.RequiredPermission + "' permission");

            if (text.Count > 0)
            {
                var textDrawing = new TextDrawing
                {
                    Text = text.ToArray()
                };
                AddChild(textDrawing);
            }

            var details = new List<string>();
            AddDebugInfo(details, debugPage);
            AddDetails(details, this);

            if (debugPage.Routes != null)
            {
                foreach (var route in debugPage.Routes)
                    AddChild(new RouteDrawing(route));
            }

            if (debugPage.Layout != null)
            {
                var layout = new LayoutDrawing(
                    drawing, 
                    this, 
                    debugPage.Layout, 
                    headingLevel + 1, 
                    showButtons);
                AddChild(layout);
            }

            if (!ReferenceEquals(debugPage.Scope, null))
            {
                AddHeaderButton(this, "Scope")
                    .AddChild(new DataScopeRulesDrawing(
                        drawing, 
                        this, 
                        debugPage.Scope, 
                        headingLevel + 1, 
                        false,
                        -1));
            }

            if (!ReferenceEquals(debugPage.DataContext, null))
            {
                AddHeaderButton(this, "Context")
                    .AddChild(new DataScopeRulesDrawing(
                        drawing,
                        this,
                        debugPage.DataContext,
                        headingLevel + 1,
                        false,
                        -1));
            }
        }

        protected override void ArrangeChildren()
        {
            ArrangeChildrenVertically(8);
        }
    }
}