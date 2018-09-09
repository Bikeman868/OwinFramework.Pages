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
            CssClass = "supplied-dependency";

            if (suppliedDependency.DataTypeSupplied != null && suppliedDependency.DataTypeSupplied.HasData())
                AddChild(new TextDrawing { Text = new[] { "Supply of " + suppliedDependency.DataTypeSupplied } });

            if (suppliedDependency.Supplier != null && suppliedDependency.Supplier.HasData())
                AddChild(new SupplierDrawing(suppliedDependency.Supplier));

            if (suppliedDependency.DataSupply != null && suppliedDependency.DataSupply.HasData())
                AddChild(new SupplyDrawing(suppliedDependency.DataSupply));

            if (suppliedDependency.DependentSupplies != null && suppliedDependency.DependentSupplies.Count > 0)
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