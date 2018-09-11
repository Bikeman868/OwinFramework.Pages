using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.DataModel;

namespace OwinFramework.Pages.Html.Elements
{
    internal class PageElementDependencies
    {
        public IDictionaryFactory DictionaryFactory;
        public IDataDependencyFactory DataDependencyFactory;
    }
}
