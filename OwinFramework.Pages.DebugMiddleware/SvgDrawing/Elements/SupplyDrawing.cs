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
            
            var description = supply.IsStatic ? "Static" : "Dynamic";

            if (supply.SuppliedData != null)
                description += " supply of " + supply.SuppliedData;

            if (supply.SubscriberCount > 0)
                description += " with " + supply.SubscriberCount + " subscribers";

            var lines = new List<string> { description };

            if (supply.Instance != null)
                lines.Add(supply.Instance.GetType().DisplayName());

            AddChild(new TextDrawing { Text = lines.ToArray() });

            if (supply.Supplier != null)
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