using System.Collections.Generic;
using System.Linq;
using Svg;
using Svg.Transforms;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes
{
    internal class TextDrawing: DrawingElement
    {
        public List<string> Text = new List<string>();

        public override void CalculateSize()
        {
            base.CalculateSize();

            var minimumHeight = DebugSvgDrawing.SvgTextLineSpacing * Text.Count + TopMargin + BottomMargin;
            var minimumWidth = Text.Max(t => t.Length) * DebugSvgDrawing.SvgTextCharacterSpacing + LeftMargin + RightMargin;

            if (Height < minimumHeight) Height = minimumHeight;
            if (Width < minimumWidth) Width = minimumWidth;
        }

        public override SvgElement Draw()
        {
            var container = base.Draw();

            for (var lineNumber = 0; lineNumber < Text.Count; lineNumber++)
            {
                var text = new SvgText(Text[lineNumber]);
                text.Transforms.Add(new SvgTranslate(LeftMargin, TopMargin + DebugSvgDrawing.SvgTextHeight + DebugSvgDrawing.SvgTextLineSpacing * lineNumber));
                text.Children.Add(new SvgTextSpan());
                container.Children.Add(text);
            }

            return container;
        }
    }
}