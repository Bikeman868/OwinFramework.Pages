using System.Collections.Generic;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Elements
{
    internal class PageDrawing : ElementDrawing
    {
        public PageDrawing(IDebugDrawing drawing, DebugPage debugPage)
            : base(
            null, 
            "Page '" + debugPage.Name + "'",
            1,
            false,
            debugPage.Scope != null)
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
                var layout = new LayoutDrawing(drawing, this, debugPage.Layout);
                AddChild(layout);
            }

            if (!ReferenceEquals(debugPage.Scope, null))
            {
                var popup = AddDataButton(this);
                popup.AddChild(new DataScopeProviderDrawing(drawing, this, debugPage.Scope));
            }
        }

        protected override void ArrangeChildren()
        {
            ArrangeChildrenVertically(8);
        }
    }
}