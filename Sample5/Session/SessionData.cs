using OwinFramework.InterfacesV1.Facilities;
using OwinFramework.InterfacesV1.Middleware;
using OwinFramework.MiddlewareHelpers.Identification;
using System.Collections.Generic;

namespace Sample5.Session
{
    internal class SessionData: ISessionData
    {
        private readonly ISession _session;

        public SessionData(ISession session)
        {
            _session = session;
        }

        public string IdentificationIdentity => _session.Get<string>("form-identification-identity");
        public List<string> IdentificationPurpose => _session.Get<List<string>>("form-identification-purpose");
        public AuthenticationStatus IdentificationStatus => _session.Get<AuthenticationStatus>("form-identification-status");
        public string IdentificationRememberMe => _session.Get<string>("form-identification-rememberme");
        public List<IdentityClaim> IdentificationClaims => _session.Get<List<IdentityClaim>>("form-identification-claims");
        public bool IdentificationIsAnonymous => _session.Get<bool>("form-identification-anonymous");
        public string IdentificationMessage => _session.Get<string>("form-identification-message");
    }
}