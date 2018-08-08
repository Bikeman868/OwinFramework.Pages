using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.DebugMiddleware.SvgDrawing.Elements;
using OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes;
using Svg;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing
{
    internal class DebugSvgDrawing : IDebugDrawing
    {
        public const float SvgTextHeight = 12;
        public const float SvgTextLineSpacing = 15;
        public const float SvgTextCharacterSpacing = 7;

        public Task Write(IOwinContext context, DebugInfo debugInfo)
        {
            var drawing = GetDrawing(debugInfo);

            string svg;
            using (var stream = new MemoryStream())
            {
                drawing.Write(stream);
                svg = Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);
            }

            context.Response.ContentType = "image/svg+xml";
            return context.Response.WriteAsync(svg);
        }

        private SvgDocument GetDrawing(DebugInfo debugInfo)
        {
            var drawing = DrawRunnableDebugInfo(debugInfo);
            drawing.SortDescendentsByZOrder();
            drawing.Arrange();
            drawing.PositionPopups();
            drawing.ArrangeMargins();
            var rootElement = drawing.Draw();

            var document = CreateSvg();
            document.Children.Add(rootElement);

            SetSize(document, drawing.Left + drawing.Width, drawing.Top + drawing.Height);

            return document;
        }

        public DrawingElement DrawRunnableDebugInfo(DebugInfo debugInfo)
        {
            if (debugInfo is DebugPage) return new PageDrawing(this, (DebugPage)debugInfo);

            return new TextDrawing
            {
                Text = new []{ debugInfo.GetType().DisplayName() }
            };
        }

        public DrawingElement DrawDebugInfo(DrawingElement page, DebugInfo debugInfo)
        {
            if (debugInfo is DebugLayout) return new LayoutDrawing(this, page, (DebugLayout)debugInfo);
            if (debugInfo is DebugRegion) return new RegionDrawing(this, page, (DebugRegion)debugInfo);
            if (debugInfo is DebugComponent) return new ComponentDrawing(this, page, (DebugComponent)debugInfo);

            return new TextDrawing 
            { 
                Text = new []{ debugInfo.GetType().DisplayName() } 
            };
        }

        #region Document creation

        private SvgDocument CreateSvg()
        {
            var document = new SvgDocument
            {
                FontFamily = "Arial",
                FontSize = 12
            };

            var styles = GetTextResource("svg.css");
            if (!string.IsNullOrEmpty(styles))
            {
                var styleElement = new NonSvgElement("style")
                {
                    Content = "\n" + styles
                };
                document.Children.Add(styleElement);
            }

            var script = GetTextResource("svg.js");
            if (!string.IsNullOrEmpty(script))
            {
                document.CustomAttributes.Add("onload", "init(evt)");
                var scriptElement = new NonSvgElement("script");
                scriptElement.CustomAttributes.Add("type", "text/ecmascript");
                scriptElement.Content = "\n" + script;
                document.Children.Add(scriptElement);
            }

            return document;
        }

        private void SetSize(SvgDocument document, SvgUnit width, SvgUnit height)
        {
            document.Width = width;
            document.Height = height;
            document.ViewBox = new SvgViewBox(0, 0, width, height);
        }

        #endregion

        #region Embedded resources

        private string GetTextResource(string filename)
        {
            var scriptResourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames().FirstOrDefault(n => n.Contains(filename));
            if (scriptResourceName != null)
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(scriptResourceName))
                {
                    if (stream == null) return null;
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            return null;
        }

        #endregion
    }
}
