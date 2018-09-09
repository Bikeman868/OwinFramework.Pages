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

            if (!string.IsNullOrEmpty(supplier.Name))
                lines.Add("Supplier '" + supplier.Name + "'");

            if (supplier.Instance != null)
                lines.Add("Implemented by " + supplier.Instance.GetType().DisplayName());

            if (supplier.DefaultSupply != null)
                lines.Add("Default supply is " + supplier.DefaultSupply);

            if (supplier.SuppliedTypes != null && supplier.SuppliedTypes.Count > 0)
            {
                var otherTypes = supplier.SuppliedTypes
                    .Where(t => (supplier.DefaultSupply == null) || (t != supplier.DefaultSupply.DataType));

                lines.AddRange(
                    otherTypes.Select(t => 
                        "Can supply " + t.DisplayName(TypeExtensions.NamespaceOption.None)));
            }

            AddChild(new TextDrawing { Text = lines.ToArray() });
        }

        protected override void ArrangeChildren()
        {
            ArrangeChildrenVertically(3);
        }
    }
}