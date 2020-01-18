using OwinFramework.InterfacesV1.Facilities;
using OwinFramework.MiddlewareHelpers.Identification;
using System.Collections.Generic;

namespace Sample5.Session
{
    internal interface ISessionData
    {
        string IdentificationIdentity { get; }
        List<string> IdentificationPurpose { get; }
        AuthenticationStatus IdentificationStatus { get; }
        string IdentificationRememberMe { get; }
        List<IdentityClaim> IdentificationClaims { get; }
        bool IdentificationIsAnonymous { get; }
        string IdentificationMessage { get; }
    }
}