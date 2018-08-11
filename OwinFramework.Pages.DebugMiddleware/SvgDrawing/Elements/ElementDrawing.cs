using System;
using System.Collections.Generic;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes;
using Svg;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Elements
{
    internal class ElementDrawing : RectangleDrawing
    {
        protected readonly DrawingElement Header;
        protected readonly DrawingElement Title;

        protected readonly DrawingElement ClassButton;
        protected readonly PopupBoxDrawing ClassPopup;

        protected readonly DrawingElement DataButton;
        protected readonly PopupBoxDrawing DataPopup;

        public ElementDrawing(
            DrawingElement page, 
            string title, 
            int headingLevel = 2,
            bool hasClass = true,
            bool hasData = false)
        {
            CornerRadius = 3f;

            Header = new HorizontalListDrawing();
            AddChild(Header);

            Title = new TextDrawing { Text = new[] { title } }.HeadingLevel(headingLevel);
            Header.AddChild(Title);

            if (!ReferenceEquals(page, null))
            {
                if (hasClass)
                {
                    ClassButton = new ButtonDrawing();
                    ClassButton.AddChild(new TextDrawing { Text = new[] { "Class" }, CssClass = "button" });
                    Header.AddChild(ClassButton);

                    ClassPopup = new PopupBoxDrawing();
                    ClassPopup.AddChild(new TextDrawing { Text = new[] { title } });
                    page.AddChild(ClassPopup);
                }

                if (hasData)
                {
                    DataButton = new ButtonDrawing();
                    DataButton.AddChild(new TextDrawing { Text = new[] { "Data" }, CssClass = "button" });
                    Header.AddChild(DataButton);

                    DataPopup = new PopupBoxDrawing();
                    DataPopup.AddChild(new TextDrawing { Text = new[] { title } });
                    page.AddChild(DataPopup);
                }
            }
        }

        protected override void ArrangeChildren()
        {
            ArrangeChildrenVertically(5);
        }

        public override void PositionPopups()
        {
            if (!ReferenceEquals(ClassPopup, null))
            {
                float left, top;
                ClassButton.GetAbsolutePosition(out left, out top);
                ClassPopup.SetAbsolutePosition(left, top + ClassButton.Height);
            }

            if (!ReferenceEquals(DataPopup, null))
            {
                float left, top;
                DataButton.GetAbsolutePosition(out left, out top);
                DataPopup.SetAbsolutePosition(left, top + DataButton.Height);
            }

            base.PositionPopups();
        }

        public override SvgElement Draw()
        {
            var drawing = base.Draw();

            if (!ReferenceEquals(ClassPopup, null))
                ClassPopup.Attach(ClassButton);

            if (!ReferenceEquals(DataPopup, null))
                DataPopup.Attach(DataButton);

            return drawing;
        }

        protected void AddDetails(List<string> details, DrawingElement parent)
        {
            if (details.Count > 0)
            {
                parent.AddChild(new TextDetailsDrawing { Text = details.ToArray() });
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
                    text.Add("Depends on " + component.GetDebugInfo());
            }
        }

        protected void AddDataConsumer(List<string> text, DebugDataConsumer consumer)
        {
            if (!ReferenceEquals(consumer, null))
                text.AddRange(consumer.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None));
        }
    }
}