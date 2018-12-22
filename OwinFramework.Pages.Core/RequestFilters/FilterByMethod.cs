using System;
using System.Linq;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Enums;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.RequestFilters
{
    /// <summary>
    /// A request filter that matches all requests for any of the specified methods
    /// </summary>
    public class FilterByMethod: IRequestFilter
    {
        private readonly Func<string, bool> _matchFunc;
        private readonly string _description;

        /// <summary>
        /// Constructs a filter that will accept any request with a matching http method
        /// </summary>
        /// <param name="methods">A list of the methods to accept</param>
        public FilterByMethod(params Method[] methods)
        {
            if (methods == null || methods.Length == 0)
            {
                _matchFunc = method => false;
                _description = "None";
            }
            else if (methods.Length == 1)
            {
                var match = string.Intern(methods[0].ToString().ToUpper());
                _matchFunc = method => method == match;
                _description = match;
            }
            else
            {
                var matches = methods.Select(m => string.Intern(m.ToString().ToUpper())).ToList();
                _matchFunc = method => matches.Any(m => m == method);
                _description = string.Join(", ", matches);
            }
        }

        string IRequestFilter.Description
        {
            get { return _description; }
        }

        /// <summary>
        /// Returns true if the request matches any of the configured methods
        /// </summary>
        public bool IsMatch(IOwinContext context, string absolutePath, string method)
        {
            return _matchFunc(method);
        }
    }
}
