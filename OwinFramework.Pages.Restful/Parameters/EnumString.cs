using System;
using System.Collections.Generic;
using OwinFramework.InterfacesV1.Capability;
using OwinFramework.Pages.Core.Interfaces.Capability;

namespace OwinFramework.Pages.Restful.Parameters
{
    /// <summary>
    /// Validates that a service endpoint parameter is a specific Enum name
    /// </summary>
    public class EnumString<T>: IParameterParser, IDocumented where T: struct, Enum
    {
        public EnumString()
        {
            _description = "Must be one of: " + string.Join(", ", Enum.GetNames(typeof(T)));
        }

        private string _description;
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
                if (Enum.TryParse<T>(parameter, true, out var value))
                {
                    result.Value = value;
                    result.Success = true;
                }
                else
                {
                    result.ErrorMessage = "Failed to parse \"" + parameter + "\" as " + typeof(T).Name + ". " + _description;
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
