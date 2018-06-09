using System;
using OwinFramework.Pages.Core.Interfaces.Builder;

namespace OwinFramework.Pages.Html.Builders
{
    internal class LayoutBuilder : ILayoutBuilder
    {
        ILayoutDefinition ILayoutBuilder.Layout()
        {
            throw new NotImplementedException();
        }
    }
}
