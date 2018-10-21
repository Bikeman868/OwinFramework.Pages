using System.Text;

namespace OwinFramework.Pages.Core.Interfaces.Templates
{
    /// <summary>
    /// Defines a class that takes the contents of a template and
    /// turns it into an ITemplate instance by parsing the content
    /// </summary>
    public interface ITemplateParser
    {
        /// <summary>
        /// Parses a template and returns an ITemplate instance
        /// </summary>
        /// <param name="template">The template to parse. These are usually in a textual
        /// format that resembles Html but with custom markup, but binary formats
        /// are also supported</param>
        /// <param name="encoding">Specifies the encoding if this is a text file</param>
        /// <param name="package">Optional package used for name resolution</param>
        /// <returns>An ITemplate instance</returns>
        ITemplate Parse(byte[] template, Encoding encoding = null, IPackage package = null);
    }
}
