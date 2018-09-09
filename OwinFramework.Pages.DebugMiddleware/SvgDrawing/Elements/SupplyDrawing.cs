using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Elements
{
    internal class SupplyDrawing: RectangleDrawing
    {
        public SupplyDrawing(DebugDataSupply supply)
        {
            CssClass = "list";

            var lines = new List<string>();

            if (!string.IsNullOrEmpty(supply.Name))
                lines.Add("Supply '" + supply.Name + "'");

            var description = supply.IsStatic ? "Static" : "Dynamic";

            if (supply.SuppliedData != null && supply.SuppliedData.HasData())
                description += " supply of " + supply.SuppliedData;

            if (supply.SubscriberCount > 0)
                description += " with " + supply.SubscriberCount + " subscribers";

            lines.Add(description);

            if (supply.Instance != null)
                lines.Add("Implemented by " + supply.Instance.GetType().DisplayName());

            AddChild(new TextDrawing { Text = lines.ToArray() });

            if (supply.Supplier != null && supply.Supplier.HasData())
            {
                AddChild(new SupplierDrawing(supply.Supplier));
            }
        }

        protected override void ArrangeChildren()
        {
            ArrangeChildrenVertically(3);
        }
    }
}