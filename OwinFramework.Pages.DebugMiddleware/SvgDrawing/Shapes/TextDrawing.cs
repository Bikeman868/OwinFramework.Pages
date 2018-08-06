using System.Collections.Generic;
using System.Linq;
using Svg;
using Svg.Transforms;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes
{
    internal class TextDrawing: DrawingElement
    {
        public float TextSize = 1f;
        public string[] Text;

        public override void ArrangeMargins()
        {
            base.ArrangeMargins();

            if (Text.Length > 0)
            {
                var minimumHeight = DebugSvgDrawing.SvgTextLineSpacing * Text.Length * TextSize + TopMargin + BottomMargin;
                var minimumWidth = Text.Max(t => t.Length) * DebugSvgDrawing.SvgTextCharacterSpacing * TextSize + LeftMargin + RightMargin;

                if (Height < minimumHeight) Height = minimumHeight;
                if (Width < minimumWidth) Width = minimumWidth;
            }
        }

        public override SvgElement Draw()
        {
            var container = base.Draw();

            for (var lineNumber = 0; lineNumber < Text.Length; lineNumber++)
            {
                var text = new SvgText(Text[lineNumber]);
                text.Transforms.Add(new SvgTranslate(LeftMargin, TopMargin + DebugSvgDrawing.SvgTextHeight * TextSize + DebugSvgDrawing.SvgTextLineSpacing * lineNumber * TextSize));
                text.Children.Add(new SvgTextSpan());
                container.Children.Add(text);
            }

            return container;
        }
    }
}