using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Elements
{
    internal class DataScopeProviderDrawing : ElementDrawing
    {
        public DataScopeProviderDrawing(IDebugDrawing drawing, DrawingElement page, DebugDataScopeProvider debugDataScope)
            : base(
            page, 
            "Data scope #" + debugDataScope.Id,
            2,
            false,
            false)
        {
            CssClass = "datascope";

            if (ClassPopup != null)
            {
                var details = new List<string>();
                AddDebugInfo(details, debugDataScope);
                AddDetails(details, ClassPopup);
            }

            if (!ReferenceEquals(debugDataScope.Scopes, null) && debugDataScope.Scopes.Count > 0)
            {
                var scopeList = new TitledListDrawing(
                    "Provided scopes", 
                    debugDataScope.Scopes.Select(s => s.ToString().InitialCaps()));
                AddChild(scopeList);
            }

            if (!ReferenceEquals(debugDataScope.DataSupplies, null) && debugDataScope.DataSupplies.Count > 0)
            {
                AddChild(new TextDrawing
                {
                    CssClass = "h3",
                    Text = new[] { "Data supplied" }
                });

                foreach (var supply in debugDataScope.DataSupplies)
                    AddChild(new SuppliedDependencyDrawing(supply));
            }
        }

        protected override void ArrangeChildren()
        {
            ArrangeChildrenVertically(5);
        }
    }
}