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

        protected DrawingElement ClassButton;
        protected PopupBoxDrawing ClassPopup;

        protected DrawingElement DataButton;
        protected PopupBoxDrawing DataPopup;

        protected DrawingElement DefinitionButton;
        protected PopupBoxDrawing DefinitionPopup;

        public ElementDrawing(
            DrawingElement page, 
            string title, 
            int headingLevel = 2,
            bool hasClass = true,
            bool hasData = false,
            bool hasDefinition = false)
        {
            CornerRadius = 3f;

            Header = new HorizontalListDrawing();
            AddChild(Header);

            Title = new TextDrawing { Text = new[] { title } }.HeadingLevel(headingLevel);
            Header.AddChild(Title);

            if (!ReferenceEquals(page, null))
            {
                if (hasClass) AddClassButton(page);
                if (hasData) AddDataButton(page);
                if (hasDefinition) AddDefinitionButton(page);
            }
        }

        protected PopupBoxDrawing AddClassButton(DrawingElement page)
        {
            ClassButton = new ButtonDrawing();
            ClassButton.AddChild(new TextDrawing { Text = new[] { "Class" }, CssClass = "button" });
            Header.AddChild(ClassButton);

            ClassPopup = new PopupBoxDrawing();
            page.AddChild(ClassPopup);
            return ClassPopup;
        }

        protected PopupBoxDrawing AddDataButton(DrawingElement page)
        {
            DataButton = new ButtonDrawing();
            DataButton.AddChild(new TextDrawing { Text = new[] { "Data" }, CssClass = "button" });
            Header.AddChild(DataButton);

            DataPopup = new PopupBoxDrawing();
            page.AddChild(DataPopup);
            return DataPopup;
        }

        protected PopupBoxDrawing AddDefinitionButton(DrawingElement page)
        {
            DefinitionButton = new ButtonDrawing();
            DefinitionButton.AddChild(new TextDrawing { Text = new[] { "Definition" }, CssClass = "button" });
            Header.AddChild(DefinitionButton);

            DefinitionPopup = new PopupBoxDrawing();
            page.AddChild(DefinitionPopup);
            return DefinitionPopup;
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

            if (!ReferenceEquals(DefinitionPopup, null))
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