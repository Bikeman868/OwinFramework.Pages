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
        private readonly Func<IOwinContext, bool> _matchFunc;
        private readonly string _description;

        /// <summary>
        /// Constructs a filter that matches the request only when all of the
        /// supplied filters match
        /// </summary>
        public FilterAllFilters(params IRequestFilter[] filters)
        {
            if (filters == null || filters.Length == 0)
            {
                _matchFunc = c => false;
                _description = "None";
            }
            else if (filters.Length == 1)
            {
                _matchFunc = c => filters[0].IsMatch(c);
                _description = filters[0].Description;
            }
            else
            {
                _matchFunc = c => filters.All(filter => filter.IsMatch(c));
                _description = filters.Skip(1).Aggregate(filters[0].Description, (s, f) => s += " " + f.Description);
            }
        }

        string IRequestFilter.Description
        {
            get { return _description; }
        }

        bool IRequestFilter.IsMatch(IOwinContext context)
        {
            return _matchFunc(context);
        }
    }
}
