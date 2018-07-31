using System;
using System.Collections.Generic;
using System.Linq;
using Svg;
using Svg.Transforms;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing
{
    internal class DrawingElement
    {
        public float Left;
        public float Top;
        public float Width;
        public float Height;

        public float LeftMargin;
        public float TopMargin;
        public float RightMargin;
        public float BottomMargin;

        public int ZOrder;
        public string CssClass;

        public List<DrawingElement> Children = new List<DrawingElement>();
        public DrawingElement Parent;

        public void AddChild(DrawingElement element)
        {
            Children.Add(element);
            element.Parent = this;
        }

        public virtual void CalculateSize()
        {
            if (Children.Count > 0)
            {
                foreach (var child in Children)
                    child.CalculateSize();

                var minChildLeft = Children.Min(c => c.Left);
                var minChildTop = Children.Min(c => c.Top);

                var childLeftAdjustment = LeftMargin - minChildLeft;
                var childTopAdjustment = TopMargin - minChildTop;

                foreach (var child in Children)
                {
                    child.Left += childLeftAdjustment;
                    child.Top += childTopAdjustment;
                }

                Width = Children.Max(c => c.Left + c.Width) + RightMargin;
                Height = Children.Max(c => c.Top + c.Height) + BottomMargin;
            }
        }

        protected virtual SvgElement GetContainer(SvgDocument document)
        {
            var group = new SvgGroup();
            @group.Transforms.Add(new SvgTranslate(Left, Top));

            if (!String.IsNullOrEmpty(CssClass))
                @group.CustomAttributes.Add("class", CssClass);

            return @group;
        }

        public virtual SvgElement Draw(SvgDocument document)
        {
            var container = GetContainer(document);
            DrawChildren(document, container.Children);
            return container;
        }

        private void SortChildrenByZOrder()
        {
            foreach (var child in Children)
                child.SortChildrenByZOrder();

            Children = Children.OrderBy(c => c.ZOrder).ToList();
        }

        protected virtual void DrawChildren(SvgDocument document, SvgElementCollection parent)
        {
            SortChildrenByZOrder();

            foreach (var child in Children)
                parent.Add(child.Draw(document));
        }
    }
}