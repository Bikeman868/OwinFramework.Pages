using System.Collections.Generic;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Elements
{
    internal class RegionDrawing : ElementDrawing
    {
        private readonly DrawingElement _content;

        public RegionDrawing(IDebugDrawing drawing, DrawingElement page, DebugRegion debugRegion)
            : base(page, "Region '" + debugRegion.Name + "'")
        {
            CssClass = "region";

            string repeat = null;

            if (!ReferenceEquals(debugRegion.RepeatType, null))
            {
                repeat = debugRegion.RepeatType.DisplayName(TypeExtensions.NamespaceOption.None);

                if (!string.IsNullOrEmpty(debugRegion.RepeatScope))
                    repeat = "'" + debugRegion.RepeatScope + "' " + repeat;

                if (!string.IsNullOrEmpty(debugRegion.ListScope))
                    repeat += " from a list in '" + debugRegion.ListScope + "' scope";

                repeat = "Repeat for each " + repeat;

                AddChild(new TextDrawing { Text = new[] { repeat } });
            }


            if (debugRegion.Content != null)
            {
                var content = drawing.DrawDebugInfo(page, debugRegion.Content);
                AddChild(content);
            }

            var details = new List<string>();

            if (!string.IsNullOrEmpty(repeat))
                details.Add(repeat);

            AddDebugInfo(details, debugRegion);
            AddDetails(details, Popup);

            if (!ReferenceEquals(debugRegion.Scope, null))
            {
                Popup.AddChild(new DataScopeProviderDrawing(drawing, page, debugRegion.Scope));
            }
        }

        protected override void ArrangeChildren()
        {
            ArrangeChildrenVertically(5);
        }
    }
}