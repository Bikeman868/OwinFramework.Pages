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
            : base(page, "Data scope #" + debugDataScope.Id)
        {
            CssClass = "datascope";

            var details = new List<string>();
            AddDebugInfo(details, debugDataScope);
            AddDetails(details, Popup);

            if (!ReferenceEquals(debugDataScope.Scopes, null))
            {
                details.AddRange(debugDataScope.Scopes.Select(s => "Scope: " + s));
            }

            if (!ReferenceEquals(debugDataScope.DataSupplies, null))
            {
                details.AddRange(debugDataScope.DataSupplies.Select(s => "Supply: " + s));
            }
        }

        protected override void ArrangeChildren()
        {
            ArrangeChildrenVertically(5);
        }
    }
}