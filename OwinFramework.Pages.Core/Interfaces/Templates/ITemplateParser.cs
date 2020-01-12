using System.Text;

namespace OwinFramework.Pages.Core.Interfaces.Templates
{
    /// <summary>
    /// Represents a file or resource containing template data
    /// </summary>
    public class TemplateResource
    {
        /// <summary>
        /// The binary content of the template resource
        /// </summary>
        public byte[] Content { get; set; }

        /// <summary>
        /// When multiple resources are combined into a template this
        /// property can be used to identify which resources perform
        /// which role. For example when parsing Vue, Angular or React
        /// templates there is an html part and a JavaScript part, in 
        /// this case the ContentType can be used to figure out which
        /// part is which.
        /// This is a mime type - for example text/html
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// If this is a text file then this is the text encoding
        /// </summary>
        public Encoding Encoding { get; set; }
    }

    /// <summary>
    /// Defines a class that takes the contents of a template and
    /// turns it into an ITemplate instance by parsing the content
    /// </summary>
    public interface ITemplateParser
    {
        /// <summary>
        /// Parses a template and returns an ITemplate instance
        /// </summary>
        /// <param name="resouces">The templates to parse. These are usually in a textual
        /// format that resembles Html but with custom markup, but binary formats
        /// are also supported. In most cases this array will contain only one element
        /// because one template file results in one template implementation. This
        /// parameter is defined as an array to allow for the use case where multiple files
        /// are needed to specify the template</param>
        /// <param name="package">Optional package used for name resolution</param>
        /// <returns>An ITemplate instance</returns>
        ITemplate Parse(TemplateResource[] resources, IPackage package = null);
    }
}
