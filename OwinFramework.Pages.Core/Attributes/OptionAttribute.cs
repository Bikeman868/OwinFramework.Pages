using System;

namespace OwinFramework.Pages.Core.Attributes
{
    /// <summary>
    /// Attach this attribute to a stand-alone service that
    /// is not part of a package
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class OptionAttribute: Attribute
    {
        /// <summary>
        /// A fragment of HTML that describes an example of hoe to use this element
        /// </summary>
        public OptionType OptionType { get; set; }

        /// <summary>
        /// The name of the option that is being documented
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// An Html fragment describing the possible values for this option
        /// </summary>
        public string Html { get; set; }
    }

    /// <summary>
    /// Specifies the type of option that is being documented
    /// </summary>
    public enum OptionType
    {
        /// <summary>
        /// This is documentation for a supported Http method
        /// </summary>
        Method,

        /// <summary>
        /// This is documentation for a variable element in the path of the Url
        /// </summary>
        PathElement,

        /// <summary>
        /// This is documenting a query string parameter in the Url
        /// </summary>
        QueryString,

        /// <summary>
        /// This is documenting an Http header value
        /// </summary>
        Header
    }
}
