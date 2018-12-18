using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OwinFramework.InterfacesV1.Capability;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces.Capability;
using OwinFramework.Pages.Restful.Interfaces;

namespace OwinFramework.Pages.Restful.Parameters
{
    /// <summary>
    /// Validates a service endpoint parameter ensuring that it is
    /// parsable from a specific value type
    /// </summary>
    public class ParameterValidator: IParameterValidator, IDocumented
    {
        private readonly Type _type;
        private readonly Action<string, Result> _parser;
        private readonly bool _optional;

        /// <summary>
        /// Returns a description of what is valid for this parameter
        /// </summary>
        public virtual string Description { get { return "A " + _type.DisplayName() + " value."; } }

        /// <summary>
        /// Returns examples of valid parameter values in HTML format
        /// </summary>
        public virtual string Examples { get { return null; } }

        /// <summary>
        /// This member of IDocumented is not used
        /// </summary>
        public IList<IEndpointAttributeDocumentation> Attributes { get { return null; } }

        /// <summary>
        /// This is a base class for parameter validators that validate
        /// standard .Net framework types.
        /// </summary>
        /// <param name="type">A value type or nullable value type.
        /// Passing a nullable type here makes the parameter optional</param>
        public ParameterValidator(Type type)
        {
            _type = type;

            if (type == typeof(string))
            {
                _parser = (param, result) =>
                    {
                        result.Success = true;
                        result.Value = param;
                    };
            }

            if (type == typeof(byte))
            {
                _parser = (param, result) =>
                {
                    byte value;
                    result.Success = byte.TryParse(param, out value);
                    result.Value = value;
                };
            }

            if (type == typeof(int))
            {
                _parser = (param, result) =>
                {
                    int value;
                    result.Success = int.TryParse(param, out value);
                    result.Value = value;
                };
            }

            if (type == typeof(long))
            {
                _parser = (param, result) =>
                {
                    long value;
                    result.Success = long.TryParse(param, out value);
                    result.Value = value;
                };
            }

            if (type == typeof(float))
            {
                _parser = (param, result) =>
                {
                    float value;
                    result.Success = float.TryParse(param, out value);
                    result.Value = value;
                };
            }

            if (type == typeof(double))
            {
                _parser = (param, result) =>
                {
                    double value;
                    result.Success = double.TryParse(param, out value);
                    result.Value = value;
                };
            }

            if (type == typeof(bool))
            {
                _parser = (param, result) =>
                {
                    bool value;
                    result.Success = bool.TryParse(param, out value);
                    result.Value = value;
                };
            }

            if (type == typeof(DateTime))
            {
                _parser = (param, result) =>
                {
                    DateTime value;
                    result.Success = DateTime.TryParse(param, out value);
                    result.Value = value;
                };
            }

            if (type == typeof(byte?))
            {
                _optional = true;
                _parser = (param, result) =>
                {
                    if (string.IsNullOrEmpty(param))
                    {
                        result.Success = true;
                        result.Value = null;
                    }
                    else
                    {
                        byte value;
                        result.Success = byte.TryParse(param, out value);
                        result.Value = result.Success ? (byte?)(value) : null;
                    }
                };
            }

            if (type == typeof(int?))
            {
                _optional = true;
                _parser = (param, result) =>
                {
                    if (string.IsNullOrEmpty(param))
                    {
                        result.Success = true;
                        result.Value = null;
                    }
                    else
                    {
                        int value;
                        result.Success = int.TryParse(param, out value);
                        result.Value = result.Success ? (int?)(value) : null;
                    }
                };
            }

            if (type == typeof(long?))
            {
                _optional = true;
                _parser = (param, result) =>
                {
                    if (string.IsNullOrEmpty(param))
                    {
                        result.Success = true;
                        result.Value = null;
                    }
                    else
                    {
                        long value;
                        result.Success = long.TryParse(param, out value);
                        result.Value = result.Success ? (long?)(value) : null;
                    }
                };
            }

            if (type == typeof(float?))
            {
                _optional = true;
                _parser = (param, result) =>
                {
                    if (string.IsNullOrEmpty(param))
                    {
                        result.Success = true;
                        result.Value = null;
                    }
                    else
                    {
                        float value;
                        result.Success = float.TryParse(param, out value);
                        result.Value = result.Success ? (float?)(value) : null;
                    }
                };
            }

            if (type == typeof(double?))
            {
                _optional = true;
                _parser = (param, result) =>
                {
                    if (string.IsNullOrEmpty(param))
                    {
                        result.Success = true;
                        result.Value = null;
                    }
                    else
                    {
                        double value;
                        result.Success = double.TryParse(param, out value);
                        result.Value = result.Success ? (double?)(value) : null;
                    }
                };
            }

            if (type == typeof(bool?))
            {
                _optional = true;
                _parser = (param, result) =>
                {
                    if (string.IsNullOrEmpty(param))
                    {
                        result.Success = true;
                        result.Value = null;
                    }
                    else
                    {
                        bool value;
                        result.Success = bool.TryParse(param, out value);
                        result.Value = result.Success ? (bool?)(value) : null;
                    }
                };
            }

            if (type == typeof(DateTime?))
            {
                _optional = true;
                _parser = (param, result) =>
                {
                    if (string.IsNullOrEmpty(param))
                    {
                        result.Success = true;
                        result.Value = null;
                    }
                    else
                    {
                        DateTime value;
                        result.Success = DateTime.TryParse(param, out value);
                        result.Value = result.Success ? (DateTime?)(value) : null;
                    }
                };
            }
        }

        public virtual IParameterValidationResult Check(string parameter)
        {
            var result = new Result
            {
                Type = _type,
                Optional = _optional
            };

            if (_parser == null)
            {
                result.Success = false;
                result.ErrorMessage = "The paramater validator does not know how to parse '" + _type.DisplayName() + "'";
            }
            else
            {
                _parser(parameter, result);
            }

            return result;
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
