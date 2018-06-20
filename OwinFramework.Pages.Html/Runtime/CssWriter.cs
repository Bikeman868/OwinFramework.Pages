using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OwinFramework.Pages.Core.Interfaces;
using OwinFramework.Pages.Core.Interfaces.Collections;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Html.Runtime
{
    internal class CssWriter: ICssWriter
    {
        public System.IO.TextWriter GetTextWriter()
        {
            throw new NotImplementedException();
        }

        public void ToHtml(IHtmlWriter html)
        {
            throw new NotImplementedException();
        }

        public void ToStringBuilder(IStringBuilder stringBuilder)
        {
            throw new NotImplementedException();
        }

        public IList<string> ToLines()
        {
            throw new NotImplementedException();
        }

        public bool Indented
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool IncludeComments
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public ICssWriter WriteRule(string selector, string styles, IPackage package)
        {
            throw new NotImplementedException();
        }

        public IHtmlWriter WriteComment(string comment)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
