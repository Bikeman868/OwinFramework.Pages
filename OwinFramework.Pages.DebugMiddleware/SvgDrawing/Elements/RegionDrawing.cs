using System.Collections.Generic;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces.Runtime;
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

            if (!ReferenceEquals(debugRegion.Scope, null) && debugRegion.Scope.HasData())
            {
                var dataScopeDrawing = new DataScopeRulesDrawing(
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

            if (showButtons && !ReferenceEquals(debugRegion.Element, null))
            {
                var elementDebugInfo = debugRegion.Element.GetDebugInfo<DebugRegion>();
                if (elementDebugInfo != null && elementDebugInfo.HasData())
                {
                    AddHeaderButton(page, "Definition")
                        .AddChild(new RegionDrawing(
                            drawing,
                            page,
                            elementDebugInfo,
                            headingLevel + 1,
                            false));
                }
            }

            if (debugRegion.Children != null && 
                debugRegion.Children.Count > 0 &&
                debugRegion.Children[0] != null &&
                debugRegion.Children[0].HasData())
            {
                var content = drawing.DrawDebugInfo(page, debugRegion.Children[0], headingLevel, showButtons);
                AddChild(content);
            }
        }
    }
}