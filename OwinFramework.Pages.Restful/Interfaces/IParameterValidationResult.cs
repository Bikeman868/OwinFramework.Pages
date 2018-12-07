using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwinFramework.Pages.Restful.Interfaces
{
    public interface IParameterValidationResult
    {
        bool Success { get; set; }
        bool Optional { get; set; }
        object Value { get; set; }
        Type Type { get; set; }
        string ErrorMessage { get; set; }
    }
}
