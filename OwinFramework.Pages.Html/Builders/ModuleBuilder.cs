using System;
using OwinFramework.Pages.Core.Interfaces.Builder;

namespace OwinFramework.Pages.Html.Builders
{
    internal class ModuleBuilder: IModuleBuilder
    {
        IModuleDefinition IModuleBuilder.Module()
        {
            throw new NotImplementedException();
        }
    }
}
