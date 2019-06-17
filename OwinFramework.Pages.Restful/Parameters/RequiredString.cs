using System;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces.Capability;

namespace OwinFramework.Pages.Restful.Parameters
{
    /// <summary>
    /// Validates a service endpoint parameter ensuring that it is
    /// a string that is not empty or all whitespace
    /// </summary>
    public class RequiredString: AnyValue<string>
    {
        private readonly Func<object, bool> _check;
        private const string _errorMessage = "The string value is required";

        public override string Description { get { return base.Description +  ". The string can not be empty or whitespace only."; } }

        public override IParameterValidationResult Check(string parameter)
        {
            var result = base.Check(parameter);
            if (!result.Success) return result;

            if (string.IsNullOrWhiteSpace(parameter))
            {
                result.Success = false;
                result.ErrorMessage = _errorMessage;
            }

            return result;
        }
    }
}
