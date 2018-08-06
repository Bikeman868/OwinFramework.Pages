using System.Collections.Generic;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes;
using Svg;

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
                    repeat = debugRegion.RepeatScope + " " + repeat;

                if (!string.IsNullOrEmpty(debugRegion.ListScope))
                    repeat += " from " + debugRegion.ListScope + " scope";

                repeat = "Repeat for each " + repeat;

                AddChild(new TextDrawing { Text = new[] { repeat } });
            }


            if (debugRegion.Content != null)
            {
                var content = drawing.DrawDebugInfo(page, debugRegion.Content);
                AddChild(content);
            }

            var details = new List<string>();

            if (!ReferenceEquals(debugRegion.Instance, null))
                details.Add("Implemented by " + debugRegion.Instance.GetType().DisplayName());

            if (!string.IsNullOrEmpty(repeat))
                details.Add(repeat);

            if (debugRegion.DependentComponents != null)
            {
                foreach (var component in debugRegion.DependentComponents)
                    details.Add("Depends on " + (component.Package == null ? string.Empty : component.Package.NamespaceName + ":") + component.Name);
            }

            if (!ReferenceEquals(debugRegion.DataConsumer, null))
                details.AddRange(debugRegion.DataConsumer);

            Popup.AddChild(new TextDrawing
            {
                CssClass = "details",
                TextSize = 9f / 12f,
                Text = details.ToArray()
            });

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