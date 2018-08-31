using System.Collections.Generic;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Elements
{
    internal class RegionDrawing : ElementDrawing
    {
        private readonly DrawingElement _content;

        public RegionDrawing(
                IDebugDrawing drawing, 
                DrawingElement page, 
                DebugRegion debugRegion,
                int headingLevel,
                bool showButtons)
            : base(
                page, 
                "Region '" + debugRegion.Name + "'",
                headingLevel)
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

                if (showButtons)
                    AddChild(new TextDrawing { Text = new[] { repeat } });
            }

            var details = new List<string>();

            if (!string.IsNullOrEmpty(repeat))
                details.Add(repeat);
            AddDebugInfo(details, debugRegion);

            if (details.Count > 0)
            {
                if (showButtons)
                    AddDetails(details, AddHeaderButton(page, "Detail"));
                else
                    AddDetails(details, this);
            }

            if (!ReferenceEquals(debugRegion.Scope, null))
            {
                var dataScopeDrawing = new DataScopeProviderDrawing(
                    drawing,
                    page,
                    debugRegion.Scope,
                    headingLevel + 1,
                    false,
                    0);

                if (showButtons)
                    AddHeaderButton(page, "Data").AddChild(dataScopeDrawing);
                else
                    AddChild(dataScopeDrawing);
            }

            if (showButtons && !ReferenceEquals(debugRegion.InstanceOf, null))
            {
                AddHeaderButton(page, "Definition")
                    .AddChild(new RegionDrawing(
                        drawing,
                        page,
                        debugRegion.InstanceOf as DebugRegion,
                        headingLevel + 1,
                        false));
            }

            if (debugRegion.Content != null)
            {
                var content = drawing.DrawDebugInfo(page, debugRegion.Content, headingLevel, showButtons);
                AddChild(content);
            }
        }
    }
}