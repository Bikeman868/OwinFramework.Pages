using System;
using OwinFramework.Pages.Core.Exceptions;
using OwinFramework.Pages.Core.Extensions;
using OwinFramework.Pages.Core.Interfaces.Capability;

namespace OwinFramework.Pages.Restful.Parameters
{
    /// <summary>
    /// Parses a service endpoint parameter as an optional string
    /// </summary>
    public class OptionalString: AnyValue<string>
    {
        public override bool IsRequired { get { return false; } }
    }
}
