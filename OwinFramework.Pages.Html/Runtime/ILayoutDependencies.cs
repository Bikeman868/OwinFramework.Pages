using System;
using OwinFramework.Pages.Core.Interfaces.Collections;

namespace OwinFramework.Pages.Html.Runtime
{
    public interface ILayoutDependencies: IDisposable
    {
        IDictionaryFactory DictionaryFactory { get; }
    }
}
