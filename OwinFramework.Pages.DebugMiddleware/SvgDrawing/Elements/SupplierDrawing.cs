using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Elements
{
    internal class SupplierDrawing: RectangleDrawing
    {
        public SupplierDrawing(DebugDataSupplier supplier)
        {
            CssClass = "list";

            var lines = new List<string>();

            if (supplier.Instance != null)
                lines.Add(supplier.Instance.GetType().DisplayName());

            lines.Add(supplier.ToString());

            AddChild(new TextDrawing { Text = lines.ToArray() });
        }

        protected override void ArrangeChildren()
        {
            ArrangeChildrenVertically(3);
        }
    }
}