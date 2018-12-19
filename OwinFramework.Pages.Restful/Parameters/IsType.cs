using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwinFramework.Pages.Restful.Parameters
{
    public class IsType<T>: ParameterValidator
    {
        public IsType(): base(typeof(T))
        { }
    }
}
