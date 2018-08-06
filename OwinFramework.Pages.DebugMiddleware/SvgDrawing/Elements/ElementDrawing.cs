using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes;
using Svg;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Elements
{
    internal class ElementDrawing : RectangleDrawing
    {
        protected readonly PopupBoxDrawing Popup;
        protected readonly DrawingElement Title;

        public ElementDrawing(DrawingElement page, string title)
        {
            Title = new TextDrawing { Text = new[] { title } };
            AddChild(Title);

            if (!ReferenceEquals(page, null))
            {
                Popup = new PopupBoxDrawing();
                Popup.AddChild(new TextDrawing { Text = new[] { title } });
                page.AddChild(Popup);
            }
        }

        public override void PositionPopups()
        {
            if (!ReferenceEquals(Popup, null))
            {
                float absoluteLeft, absoluteTop;
                GetAbsolutePosition(out absoluteLeft, out absoluteTop);

                Popup.SetAbsolutePosition(absoluteLeft, absoluteTop + Title.Top + Title.Height);
            }

            base.PositionPopups();
        }

        public override SvgElement Draw()
        {
            var drawing = base.Draw();

            if (!ReferenceEquals(Popup, null))
                Popup.Attach(Title);

            return drawing;
        }

        protected void AddDetails(List<string> details, DrawingElement parent)
        {
            if (details.Count > 0)
            {
                parent.AddChild(new TextDrawing
                {
                    CssClass = "details",
                    TextSize = 9f / 12f,
                    Text = details.ToArray()
                });
            }
        }

        protected void AddDebugInfo(List<string> text, DebugInfo debugInfo)
        {
            if (!ReferenceEquals(debugInfo.Instance, null))
                text.Add("Implemented by " + debugInfo.Instance.GetType().DisplayName());
            AddDependentComponents(text, debugInfo.DependentComponents);
            AddDataConsumer(text, debugInfo.DataConsumer);
        }

        protected void AddDependentComponents(List<string> text, List<IComponent> dependentComponents)
        {
            if (!ReferenceEquals(dependentComponents, null))
            {
                foreach (var component in dependentComponents)
                    text.Add("Depends on component '" + (component.Package == null ? string.Empty : component.Package.NamespaceName + ":") + component.Name + "'");
            }
        }

        protected void AddDataConsumer(List<string> text, DebugDataConsumer consumer)
        {
            if (!ReferenceEquals(consumer, null))
                text.AddRange(consumer.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None));
        }
    }
}