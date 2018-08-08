using System.Linq;
using Svg;
using Svg.Transforms;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes
{
    internal class TextDetailsDrawing : TextDrawing
    {
        public TextDetailsDrawing()
        {
            CssClass = "details";
            TextSize = 9f / 12f;
        }
    }
}