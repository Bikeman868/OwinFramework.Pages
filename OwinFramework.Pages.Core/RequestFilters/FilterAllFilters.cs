using System;
using System.Linq;
using Microsoft.Owin;
using OwinFramework.Pages.Core.Interfaces.Runtime;

namespace OwinFramework.Pages.Core.RequestFilters
{
    /// <summary>
    /// A request filter that matches all requests where all supplied
    /// filters match
    /// </summary>
    public class FilterAllFilters: IRequestFilter
    {
        private readonly Func<IOwinContext, string, string, bool> _matchFunc;
        private readonly string _description;

        /// <summary>
        /// Constructs a filter that matches the request only when all of the
        /// supplied filters match
        /// </summary>
        public FilterAllFilters(params IRequestFilter[] filters)
        {
            if (filters == null || filters.Length == 0)
            {
                _matchFunc = (c, u, m) => false;
                _description = "None";
            }
            else if (filters.Length == 1)
            {
                _matchFunc = (c, u, m) => filters[0].IsMatch(c, u, m);
                _description = filters[0].Description;
            }
            else
            {
                _matchFunc = (c, u, m) => filters.All(filter => filter.IsMatch(c, u, m));
                _description = filters.Skip(1).Aggregate(filters[0].Description, (s, f) => s += " " + f.Description);
            }
        }

        string IRequestFilter.Description
        {
            get { return _description; }
        }

        /// <summary>
        /// Returns true if the request matches all of the filters
        /// </summary>
        public bool IsMatch(IOwinContext context, string absolutePath, string method)
        {
            return _matchFunc(context, absolutePath, method);
        }
    }
}
