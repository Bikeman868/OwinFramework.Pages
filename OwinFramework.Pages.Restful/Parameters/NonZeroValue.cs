﻿using System;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces.Capability;

namespace OwinFramework.Pages.Restful.Parameters
{
    /// <summary>
    /// Validates a service endpoint parameter ensuring that it is
    /// a number that is not zero.
    /// </summary>
    /// <typeparam name="T">The type of number</typeparam>
    public class NonZeroValue<T>: IsType<T>
    {
        private readonly Func<object, bool> _check;
        private const string _errorMessage = "The value can be any number other than zero";

        public override string Description { get { return base.Description +  ". The number can not be zero."; } }

        /// <summary>
        /// Checks service endpoint parameters to ensure that they are
        /// a number greater than zero
        /// </summary>
        public NonZeroValue() 
        {
            var type = typeof(T);

            if (type == typeof(int) || type == typeof(int?))
                _check = v => (int)v > 0;

            if (type == typeof(float) || type == typeof(float?))
                _check = v => (float)v > 0;

            if (type == typeof(double) || type == typeof(double?))
                _check = v => (double)v > 0;

            else throw new ServiceBuilderException(
                    "The type '" + type.DisplayName() + "' is not supported by the " +
                    "positive number service endpoint parameter validator");
        }

        public override IParameterValidationResult Check(string parameter)
        {
            var result = base.Check(parameter);
            if (!result.Success) return result;

            if (_check(result.Value)) return result;

            result.Success = false;
            result.ErrorMessage = _errorMessage;

            return result;
        }
    }
}
