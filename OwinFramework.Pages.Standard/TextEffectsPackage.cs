using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Builder;
using OwinFramework.Pages.Core.Interfaces.Runtime;
using OwinFramework.Pages.Html.Elements;
using OwinFramework.Pages.Html.Runtime;

namespace OwinFramework.Pages.Standard
{
    /// <summary>
    /// This package contains components for rendering text with special
    /// effects.
    /// </summary>
    public class TextEffectsPackage : Framework.Runtime.Package
    {
        /// <summary>
        /// Defines the data that must be placed in the rendering context
        /// to define how the vertical text will be rendered
        /// </summary>
        public class VerticalText
        {
            /// <summary>
            /// The text to draw vertically
            /// </summary>
            public string Caption { get; set; }

            /// <summary>
            /// The width of the text area in pixels
            /// </summary>
            public float Width { get; set; }

            /// <summary>
            /// The height of the text area in pixels
            /// </summary>
            public float Height { get; set; }

            /// <summary>
            /// The height of the text within the text area
            /// </summary>
            public float TextHeight { get; set; }

            /// <summary>
            /// The horizontal offset of the baseline of the
            /// text from the left edge of the text area
            /// </summary>
            public float X { get; set; }

            /// <summary>
            /// The vertical offset to the left edge of the first
            /// character of the text from the top of the text area
            /// </summary>
            public float Y { get; set; }

            /// <summary>
            /// Defines the fill style of the background. Set to null
            /// for no background fill
            /// </summary>
            public string Background { get; set; }

            /// <summary>
            /// CSS style to apply to the rendered text
            /// </summary>
            public string TextStyle { get; set; }

            /// <summary>
            /// Initializes the vertical text data with default values
            /// </summary>
            /// <param name="caption">The text to render</param>
            /// <param name="width">The width in pixels</param>
            /// <param name="height">The height in pixels</param>
            public VerticalText(string caption, float width, float height)
            {
                Caption = caption;
                Width = width;
                Height = height;
                TextHeight = height * 0.8f;
                X = width * 0.8f;
                Y = height * 0.9f;
                Background = "blue";
                TextStyle = "font: 30px serif; fill: white;";
            }
        }

        /// <summary>
        /// This component renders text vertically using svg
        /// </summary>
        private class VerticalTextComponent : Component
        {
            public VerticalTextComponent(IComponentDependenciesFactory dependencies)
                : base(dependencies)
            {
                PageAreas = new[] { PageArea.Body };
            }

            public override IWriteResult WritePageArea(
                IRenderContext context,
                PageArea pageArea)
            {
                if (pageArea == PageArea.Body)
                {
                    var data = context.Data.Get<VerticalText>();

                    context.Html.WriteOpenTag("svg", "width", data.Width.ToString(), "height", data.Height.ToString());
                    context.Html.WriteElementLine("style", "text { " + data.TextStyle + " }");

                    if (!string.IsNullOrEmpty(data.Background))
                        context.Html.WriteOpenTag("rect", true, "width", "100%", "height", "100%", "fill", data.Background);

                    context.Html.WriteOpenTag("g", "transform", "translate(" + data.X + "," + data.Y + ") rotate(-90)");
                    context.Html.WriteElementLine("text", data.Caption, "textLength", data.TextHeight.ToString(), "lengthAdjust", "spacingAndGlyphs");
                    context.Html.WriteCloseTag("g");

                    context.Html.WriteCloseTag("svg");
                }
                return WriteResult.Continue();
            }
        }

        public TextEffectsPackage(IPackageDependenciesFactory dependencies)
            : base(dependencies) { }

        /// <summary>
        /// This is the method that builds all of the compnents in the text effects package
        /// </summary>
        public override IPackage Build(IFluentBuilder builder)
        {
            builder.BuildUpComponent(new VerticalTextComponent(Dependencies.ComponentDependenciesFactory))
                .Name("verticalText")
                .BindTo<VerticalText>()
                .Build();

            return this;
        }
    }
}
