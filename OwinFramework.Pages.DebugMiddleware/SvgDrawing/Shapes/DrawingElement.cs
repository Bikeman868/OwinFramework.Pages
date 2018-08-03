using System;
using System.Collections.Generic;
using System.Linq;
using Svg;
using Svg.Transforms;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes
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

        public SvgElement Container;

        public void AddChild(DrawingElement element)
        {
            element.Arrange();
            Children.Add(element);
            element.Parent = this;
        }

        public virtual void Arrange()
        {
            if (Children.Count > 0)
            {
                foreach (var child in Children)
                    child.Arrange();

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

        protected virtual SvgElement GetContainer()
        {
            var container = new SvgGroup();
            container.Transforms.Add(new SvgTranslate(Left, Top));

            if (!String.IsNullOrEmpty(CssClass))
                container.CustomAttributes.Add("class", CssClass);

            return container;
        }

        public virtual SvgElement Draw()
        {
            Container = GetContainer();
            DrawChildren(Container.Children);
            return Container;
        }

        public void SortChildrenByZOrder()
        {
            foreach (var child in Children)
                child.SortChildrenByZOrder();

            Children = Children.OrderBy(c => c.ZOrder).ToList();
        }

        public void GetAbsolutePosition(out float left, out float top)
        {
            left = Left;
            top = Top;

            var parent = Parent;
            while (parent != null)
            {
                left += Parent.Left;
                top += Parent.Top;
                parent = parent.Parent;
            }
        }

        public void SetAbsolutePosition(float left, float top)
        {
            float currentLeft, currentTop;
            GetAbsolutePosition(out currentLeft, out currentTop);
            Left += left - currentLeft;
            Top += top - currentTop;
        }

        protected virtual void DrawChildren(SvgElementCollection parent)
        {
            foreach (var child in Children)
                parent.Add(child.Draw());
        }
    }
}