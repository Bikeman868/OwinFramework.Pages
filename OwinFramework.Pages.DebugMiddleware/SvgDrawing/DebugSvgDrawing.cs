using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Extensions;
using Svg;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing
{
    internal class DebugSvgDrawing
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
            var drawing = DrawDebugInfo(debugInfo);

            var document = CreateSvg();
            drawing.Draw(document);

            SetSize(document, drawing.Left + drawing.Width, drawing.Top + drawing.Height);
            Finalize(document);

            return document;
        }

        public DrawingElement DrawDebugInfo(DebugInfo debugInfo)
        {
            if (debugInfo is DebugPage) return new PageDrawing(this, (DebugPage)debugInfo);
            if (debugInfo is DebugLayout) return new LayoutDrawing(this, (DebugLayout)debugInfo);
            if (debugInfo is DebugRegion) return new RegionDrawing(this, (DebugRegion)debugInfo);
            if (debugInfo is DebugComponent) return new ComponentDrawing(this, (DebugComponent)debugInfo);

            return new TextDrawing 
            { 
                Text = new List<string> 
                { 
                    "Unknown type of debugInfo", 
                    debugInfo.GetType().DisplayName()
                } 
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

        private void Finalize(SvgDocument document)
        {
            //if (document != null)
            //{
            //    var elements = new SvgElement[document.Children.Count];
            //    document.Children.CopyTo(elements, 0);

            //    document.Children.Clear();

            //    foreach (var element in elements.Where(e => !e.ContainsAttribute("visibility")))
            //        document.Children.Add(element);

            //    foreach (var element in elements.Where(e => e.ContainsAttribute("visibility")))
            //        document.Children.Add(element);
            //}
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
