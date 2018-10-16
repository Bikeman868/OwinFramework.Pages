using System;

namespace OwinFramework.Pages.Core.Interfaces.Templates
{
    /// <summary>
    /// Defines a fluent interface for creating templates
    /// </summary>
    public interface ITemplateDefinition
    {
        /// <summary>
        /// Sets the namespace used to resolve component references
        /// </summary>
        ITemplateDefinition PartOf(string packageName);

        /// <summary>
        /// Sets the namespace used to resolve component references
        /// </summary>
        ITemplateDefinition PartOf(IPackage package);

        /// <summary>
        /// Adds a block of HTML to the template definition
        /// </summary>
        ITemplateDefinition AddHtml(string html);

        /// <summary>
        /// Adds an opening Html element
        /// </summary>
        ITemplateDefinition AddElementOpen(string tag, params string[] attributePairs);

        /// <summary>
        /// Adds an attribute to the most recently added element. The attribute
        /// value is supplied by reading a property from data in the render context.
        /// You can only call this method after calling AddElementOpen() and before
        /// calling AddElementClose().
        /// </summary>
        /// <param name="attributeName">The name of the attribute to set</param>
        /// <param name="dataType">The type of data to bind to. This will typically 
        /// be an object with public properties</param>
        /// <param name="propertyName">The name of the public property to read from 
        /// the data obkect</param>
        /// <param name="dataFormatter">Optional data formatter that will take the 
        /// property value and convert it to text. If you do do not pass a formatter 
        /// then the ToString() method will be called on the property value</param>
        /// <param name="scopeName">Optional scope name to use when resolving
        /// data from the data context</param>
        ITemplateDefinition SetElementAttribute(
            string attributeName, 
            Type dataType, 
            string propertyName, 
            IDataFieldFormatter dataFormatter = null, 
            string scopeName = null);

        /// <summary>
        /// Closes a previously opened Html element
        /// </summary>
        ITemplateDefinition AddElementClose();

        /// <summary>
        /// Adds a reference to a layout
        /// </summary>
        ITemplateDefinition AddLayout(string layoutName);

        /// <summary>
        /// Adds a reference to a layout
        /// </summary>
        ITemplateDefinition AddLayout(ILayout layout);

        /// <summary>
        /// Adds a reference to a region
        /// </summary>
        ITemplateDefinition AddRegion(string regionName);

        /// <summary>
        /// Adds a reference to a region
        /// </summary>
        ITemplateDefinition AddRegion(IRegion region);

        /// <summary>
        /// Adds a reference to a region
        /// </summary>
        ITemplateDefinition AddComponent(string componentName);

        /// <summary>
        /// Adds a reference to a region
        /// </summary>
        ITemplateDefinition AddComponent(IComponent component);

        /// <summary>
        /// Adds a reference to a region
        /// </summary>
        ITemplateDefinition AddTemplate(string templatePath);

        /// <summary>
        /// Adds a reference to another template
        /// </summary>
        ITemplateDefinition AddTemplate(ITemplate template);

        /// <summary>
        /// Starts a section that will repeat
        /// </summary>
        ITemplateDefinition RepeatStart(Type dataTypeToRepeat);

        /// <summary>
        /// Closes off a repeating section
        /// </summary>
        ITemplateDefinition RepeatEnd();

        /// <summary>
        /// Adds data from a property of a data bound object to the Html output
        /// by the template when it is rendered.
        /// </summary>
        /// <param name="dataType">The type of data to bind to. This will typically 
        /// be an object with public properties</param>
        /// <param name="propertyName">The name of the public property to read from 
        /// the data obkect</param>
        /// <param name="dataFormatter">Optional data formatter that will take the 
        /// property value and convert it to text. If you do do not pass a formatter 
        /// then the ToString() method will be called on the property value</param>
        /// <param name="scopeName">Optional scope name to use when resolving
        /// data from the data context</param>
        ITemplateDefinition AddDataField(
            Type dataType, 
            string propertyName, 
            IDataFieldFormatter dataFormatter = null, 
            string scopeName = null);

        /// <summary>
        /// Adds a paragraph of static text that can be localized
        /// </summary>
        /// <param name="assetName">A unique name for this localizable asset. See
        /// Asset Manager for more information</param>
        /// <param name="defaultText">The default text to use for all unsupported
        /// localizations. Note that if you have not set up localization then this
        /// is the text that will always be used</param>
        ITemplateDefinition AddText(string assetName, string defaultText);

        /// <summary>
        /// Constructs and returns the template that was defined
        /// </summary>
        ITemplate Build();
    }
}
