using System.Collections.Generic;
using System.Linq;
using OwinFramework.Pages.Core.Debug;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.DebugMiddleware.SvgDrawing.Shapes;

namespace OwinFramework.Pages.DebugMiddleware.SvgDrawing.Elements
{
    internal class DataScopeProviderDrawing : ElementDrawing
    {
        public DataScopeProviderDrawing(
                IDebugDrawing drawing, 
                DrawingElement page, 
                DebugDataScopeProvider debugDataScope,
                int headingLevel,
                bool showButtons,
                int depth)
            : base(
                page, 
                "Data scope #" + debugDataScope.Id,
                headingLevel)
        {
            CssClass = "data-scope";

            var details = new List<string>();
            AddDebugInfo(details, debugDataScope);

            if (details.Count > 0)
            {
                if (showButtons)
                    AddDetails(details, AddHeaderButton(page, "Detail"));
                else
                    AddDetails(details, this);
            }

            if (!ReferenceEquals(debugDataScope.Scopes, null) && debugDataScope.Scopes.Count > 0)
            {
                var scopeList = new TitledListDrawing(
                    "Data scopes", 
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

            if (depth != 0 && debugDataScope.Children != null && debugDataScope.Children.Count > 0)
            {
                foreach(var child in debugDataScope.Children)
                {
                    var childDrawing = new DataScopeProviderDrawing(
                        drawing,
                        page,
                        child as DebugDataScopeProvider,
                        headingLevel,
                        showButtons,
                        depth - 1);
                    AddChild(childDrawing);
                }
            }
        }
    }
}