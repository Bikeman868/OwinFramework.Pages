using System;
using System.Linq;
using System.Collections.Generic;
using OwinFramework.InterfacesV1.Capability;
using OwinFramework.Pages.Core.Interfaces.Capability;

namespace OwinFramework.Pages.Restful.Parameters
{
    /// <summary>
    /// Validates that a service endpoint parameter is a specific Enum value
    /// </summary>
    public class EnumValue<T>: IParameterParser, IDocumented where T: struct, Enum
    {
        public EnumValue()
        {
            _values = Enum.GetValues(typeof(T)).Cast<int>().ToArray();
            _description = "Must be one of: " + string.Join(", ", _values.Select(v => v.ToString()));
        }

        private string _description;
        private int[] _values;

        public string Description => _description;

        string IDocumented.Examples => Enum.GetNames(typeof(T))[0];

        IList<IEndpointAttributeDocumentation> IDocumented.Attributes => null;

        bool IParameterParser.IsRequired => true;

        Type IParameterParser.ParameterType => typeof(T);

        IParameterValidationResult IParameterParser.Check(string parameter)
        {
            var result = new Result
            {
                Type = typeof(T)
            };

            if (string.IsNullOrWhiteSpace(parameter))
            {
                result.ErrorMessage = "The parameter is required and can not be blank";
            }
            else
            {
                if (int.TryParse(parameter, out var value))
                {
                    if (_values.Contains(value))
                    {
                        result.Value = Enum.ToObject(typeof(T), value);
                        result.Success = true;
                    }
                    else
                    {
                        result.ErrorMessage = "Parameter value \"" + parameter + "\" is not valid. " + _description;
                    }
                }
                else
                {
                    result.ErrorMessage = "Parameter value \"" + parameter + "\" is not a valid integer";
                }
            }
            return result;
        }

        protected class Result : IParameterValidationResult
        {
            public bool Success { get; set; }
            public object Value { get; set; }
            public Type Type { get; set; }
            public string ErrorMessage { get; set; }
        }
    }
}
