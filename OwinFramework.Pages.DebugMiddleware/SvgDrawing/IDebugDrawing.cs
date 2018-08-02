using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing
{
    internal interface IDebugDrawing
    {
        DrawingElement DrawDebugInfo(DrawingElement page, DebugInfo debugInfo);
    }
}