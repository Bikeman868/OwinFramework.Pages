using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OwinFramework.Pages.Restful.Interfaces;

namespace OwinFramework.Pages.Restful.Parameters
{
    /// <summary>
    /// Validates a service endpoint parameter ensuring that it is
    /// parsable from a specific value type
    /// </summary>
    public class ParameterValidator: IParameterValidator
    {
        private readonly Type _type;

        /// <summary>
        /// This is a base class for parameter validators that validate
        /// standard .Net framework types.
        /// </summary>
        /// <param name="type">A value type or nullable value type.
        /// Passing a nullable type here makes the parameter optional</param>
        public ParameterValidator(Type type)
        {
            _type = type;
        }

        public virtual IParameterValidationResult Check(string parameter)
        {
            return new Result
            {
                Type = _type,
                Success = true,
                Value = null
            };
        }

        private class Result: IParameterValidationResult
        {
            public bool Success { get; set; }
            public bool Optional { get; set; }
            public object Value { get; set; }
            public Type Type { get; set; }
            public string ErrorMessage { get; set; }
        }
    }
}
