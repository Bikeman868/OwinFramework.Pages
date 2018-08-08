using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Elements
{
    internal class SuppliedDependencyDrawing: RectangleDrawing
    {
        public SuppliedDependencyDrawing(DebugSuppliedDependency suppliedDependency)
        {
            CssClass = "list";

            if (suppliedDependency.DataSupplied != null)
                AddChild(new TextDrawing { Text = new[] { "Supply of " + suppliedDependency.DataSupplied } });

            if (suppliedDependency.Supply != null)
            {
                AddChild(new SupplyDrawing(suppliedDependency.Supply));
            }
            else if (suppliedDependency.Supplier != null)
            {
                AddChild(new SupplierDrawing(suppliedDependency.Supplier));
            }

            if (suppliedDependency.DependentSupplies != null)
            {
                var dependencies = suppliedDependency.DependentSupplies.Select(s => "Depends on " + s.ToString());
                AddChild(new TitledListDrawing(null, dependencies));
            }
        }

        protected override void ArrangeChildren()
        {
            ArrangeChildrenVertically(3);
        }
    }
}