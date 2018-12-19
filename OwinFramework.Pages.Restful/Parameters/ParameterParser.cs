using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.InterfacesV1.Capability;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces.Capability;

namespace OwinFramework.Pages.Restful.Parameters
{
    /// <summary>
    /// Validates a service endpoint parameter ensuring that it is
    /// parsable from a specific value type
    /// </summary>
    public class ParameterParser: IParameterParser, IDocumented
    {
        private readonly Action<string, Result> _parser;
        private readonly bool _optional;
        private readonly string _description;
        private readonly Type _type;
        private readonly string _example;

        /// <summary>
        /// The type of object that will be returned by this parser
        /// </summary>
        public virtual Type ParameterType { get { return _type; } }
        
        /// <summary>
        /// Returns a description of what is valid for this parameter
        /// </summary>
        public virtual string Description { get { return _description; } }

        /// <summary>
        /// Returns examples of valid parameter values in HTML format
        /// </summary>
        public virtual string Examples { get { return _example; } }

        /// <summary>
        /// If required parameters are not provided in the request then a
        /// 'Bad Request' response is sent back to the caller
        /// </summary>
        public virtual bool IsRequired { get { return !_optional; } }

        /// <summary>
        /// This member of IDocumented is not used
        /// </summary>
        public IList<IEndpointAttributeDocumentation> Attributes { get { return null; } }

        /// <summary>
        /// This is a base class for parameter parsers that parse and validate
        /// standard .Net framework value types.
        /// </summary>
        public ParameterParser(Type type)
        {
            _type = type;
            _example = "54";

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                _optional = true;
                type = Nullable.GetUnderlyingType(type);

                if (type == typeof(byte))
                {
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
                            if (byte.TryParse(param, out value))
                            {
                                result.Success = true;
                                result.Value = (byte?)value;
                            }
                            else
                            {
                                result.ErrorMessage = "Failed to parse as a byte";
                            }
                        }
                    };
                }
                else if (type == typeof(int))
                {
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
                            if (int.TryParse(param, out value))
                            {
                                result.Success = true;
                                result.Value = (int?)value;
                            }
                            else
                            {
                                result.ErrorMessage = "Failed to parse as an integer";
                            }
                        }
                    };
                }
                else if (type == typeof(long))
                {
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
                            if (long.TryParse(param, out value))
                            {
                                result.Success = true;
                                result.Value = (long?)value;
                            }
                            else
                            {
                                result.ErrorMessage = "Failed to parse as an integer";
                            }
                        }
                    };
                }
                else if (type == typeof(float))
                {
                    _example = "12.45";
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
                            if (float.TryParse(param, out value))
                            {
                                result.Success = true;
                                result.Value = (float?)value;
                            }
                            else
                            {
                                result.ErrorMessage = "Failed to parse as a float";
                            }
                        }
                    };
                }
                else if (type == typeof(double))
                {
                    _example = "18.34568";
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
                            if (double.TryParse(param, out value))
                            {
                                result.Success = true;
                                result.Value = (double?)value;
                            }
                            else
                            {
                                result.ErrorMessage = "Failed to parse as a double";
                            }
                        }
                    };
                }
                else if (type == typeof(bool))
                {
                    _example = "true";
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
                            if (bool.TryParse(param, out value))
                            {
                                result.Success = true;
                                result.Value = (bool?)value;
                            }
                            else
                            {
                                result.ErrorMessage = "Failed to parse as a bool";
                            }
                        }
                    };
                }
                else if (type == typeof(DateTime))
                {
                    _example = DateTime.Now.ToString("r");
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
                            if (DateTime.TryParse(param, out value))
                            {
                                result.Success = true;
                                result.Value = (DateTime?)value;
                            }
                            else
                            {
                                result.ErrorMessage = "Failed to parse as a DateTime";
                            }
                        }
                    };
                }
                else if (type.IsEnum)
                {
                    _example = Enum.GetNames(type)[0];
                    _parser = (param, result) =>
                    {
                        if (string.IsNullOrEmpty(param))
                        {
                            result.Success = true;
                            result.Value = null;
                        }
                        else
                        {
                            try
                            {
                                result.Value = Enum.Parse(type, param, true);
                                result.Success = true;
                            }
                            catch (Exception e)
                            {
                                result.ErrorMessage = e.Message;
                            }
                        }
                    };
                    _description = "Optional, can be one of: " + string.Join(", ", Enum.GetNames(type));
                }
            }
            else
            {
                if (type == typeof(string))
                {
                    _example = "some_text";
                    _parser = (param, result) =>
                    {
                        result.Success = true;
                        result.Value = param;
                    };
                }
                else if (type == typeof(byte))
                {
                    _parser = (param, result) =>
                    {
                        byte value;
                        if (byte.TryParse(param, out value))
                        {
                            result.Success = true;
                            result.Value = value;
                        }
                        else
                        {
                            result.ErrorMessage = "Failed to parse as a bool";
                        }
                    };
                }
                else if (type == typeof(int))
                {
                    _parser = (param, result) =>
                    {
                        int value;
                        if (int.TryParse(param, out value))
                        {
                            result.Success = true;
                            result.Value = value;
                        }
                        else
                        {
                            result.ErrorMessage = "Failed to parse as a int";
                        }
                    };
                }
                else if (type == typeof(long))
                {
                    _parser = (param, result) =>
                    {
                        long value;
                        if (long.TryParse(param, out value))
                        {
                            result.Success = true;
                            result.Value = value;
                        }
                        else
                        {
                            result.ErrorMessage = "Failed to parse as a long";
                        }
                    };
                }
                else if (type == typeof(float))
                {
                    _example = "3.14159267";
                    _parser = (param, result) =>
                    {
                        float value;
                        if (float.TryParse(param, out value))
                        {
                            result.Success = true;
                            result.Value = value;
                        }
                        else
                        {
                            result.ErrorMessage = "Failed to parse as a float";
                        }
                    };
                }
                else if (type == typeof(double))
                {
                    _example = "3.14159267";
                    _parser = (param, result) =>
                    {
                        double value;
                        if (double.TryParse(param, out value))
                        {
                            result.Success = true;
                            result.Value = value;
                        }
                        else
                        {
                            result.ErrorMessage = "Failed to parse as a double";
                        }
                    };
                }
                else if (type == typeof(bool))
                {
                    _example = "false";
                    _parser = (param, result) =>
                    {
                        bool value;
                        if (bool.TryParse(param, out value))
                        {
                            result.Success = true;
                            result.Value = value;
                        }
                        else
                        {
                            result.ErrorMessage = "Failed to parse as a bool";
                        }
                    };
                }
                else if (type == typeof(DateTime))
                {
                    _example = DateTime.Now.ToString("r");
                    _parser = (param, result) =>
                    {
                        DateTime value;
                        if (DateTime.TryParse(param, out value))
                        {
                            result.Success = true;
                            result.Value = value;
                        }
                        else
                        {
                            result.ErrorMessage = "Failed to parse as a DateTime";
                        }
                    };
                }
                else if (type.IsEnum)
                {
                    _example = Enum.GetNames(type)[0];
                    _parser = (param, result) =>
                    {
                        try
                        {
                            result.Value = Enum.Parse(type, param, true);
                            result.Success = true;
                        }
                        catch (Exception e)
                        {
                            result.ErrorMessage = e.Message;
                        }
                    };

                    _description = "Must be one of: " + string.Join(", ", Enum.GetNames(type));
                }
            }

            if (_description == null)
                _description = (_optional ? "Optional " : "Required ") + " '" + type.DisplayName() + "' value";
        }

        public virtual IParameterValidationResult Check(string parameter)
        {
            var result = new Result
            {
                Type = _type
            };

            if (_parser == null)
            {
                result.Success = false;
                result.ErrorMessage = "The paramater parser does not know how to parse '" + 
                    _type.DisplayName() + "', if this is the type you intended to use then you " + 
                    "must write a custom parser for this type";
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
            public object Value { get; set; }
            public Type Type { get; set; }
            public string ErrorMessage { get; set; }
        }
    }
}
