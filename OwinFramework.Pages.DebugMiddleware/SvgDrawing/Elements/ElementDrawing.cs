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

            Popup = new PopupBoxDrawing();
            Popup.AddChild(new TextDrawing { Text = new[] { title } });
            page.AddChild(Popup);
        }

        public override void PositionPopups()
        {
            float absoluteLeft, absoluteTop;
            GetAbsolutePosition(out absoluteLeft, out absoluteTop);

            Popup.SetAbsolutePosition(absoluteLeft, absoluteTop + Title.Top + Title.Height);

            base.PositionPopups();
        }

        public override SvgElement Draw()
        {
            var drawing = base.Draw();
            Popup.Attach(Title);
            return drawing;
        }
    }
}