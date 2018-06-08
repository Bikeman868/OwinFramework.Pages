using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Facilities.Runtime
{
    internal class HtmlWriterFactory: IHtmlWriterFactory
    {
        private readonly IStringBuilderFactory _stringBuilderFactory;

        public HtmlWriterFactory(
            IStringBuilderFactory stringBuilderFactory)
        {
            _stringBuilderFactory = stringBuilderFactory;
        }

        IHtmlWriter IHtmlWriterFactory.Create(bool indented)
        {
            return new HtmlWriter(_stringBuilderFactory) { Indented = indented };
        }
    }
}
