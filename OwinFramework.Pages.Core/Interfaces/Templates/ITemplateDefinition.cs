using System;
using System.Linq.Expressions;

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
        /// Specifies that this component is deployed as part of a module
        /// </summary>
        /// <param name="module">The module that this component is deployed in</param>
        ITemplateDefinition DeployIn(IModule module);

        /// <summary>
        /// Specifies that this layout is deployed as part of a module
        /// </summary>
        /// <param name="moduleName">The name of the module that this 
        /// layout is deployed in</param>
        ITemplateDefinition DeployIn(string moduleName);

        /// <summary>
        /// Adds a block of HTML to the template definition
        /// </summary>
        ITemplateDefinition AddHtml(string html);

        /// <summary>
        /// Adds an html element with attributes and content
        /// </summary>
        ITemplateDefinition AddElement(string tag, string content, params string[] attributePairs);

        /// <summary>
        /// Adds an html element with attributes and content
        /// </summary>
        ITemplateDefinition AddSelfClosingElement(string tag, params string[] attributePairs);

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
        /// Starts a new line in the Html
        /// </summary>
        ITemplateDefinition AddLineBreak();

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
        /// Adds a reference to a component
        /// </summary>
        ITemplateDefinition AddComponent(string componentName);

        /// <summary>
        /// Adds a reference to a component
        /// </summary>
        ITemplateDefinition AddComponent(IComponent component);

        /// <summary>
        /// Adds a reference to another template
        /// </summary>
        ITemplateDefinition AddTemplate(string templatePath);

        /// <summary>
        /// Adds a reference to another template
        /// </summary>
        ITemplateDefinition AddTemplate(ITemplate template);

        /// <summary>
        /// Starts a section that will repeat
        /// </summary>
        ITemplateDefinition RepeatStart(Type dataTypeToRepeat, string scopeName = null, string listScopeName = null);

        /// <summary>
        /// Starts a section that will repeat
        /// </summary>
        ITemplateDefinition RepeatStart<T>(string scopeName = null, string listScopeName = null);

        /// <summary>
        /// Closes off a repeating section
        /// </summary>
        ITemplateDefinition RepeatEnd();

        /// <summary>
        /// Finds a supplier of the specified type of data and adds it to the
        /// render context for the purpose of data binding
        /// </summary>
        /// <param name="dataType">The type of data to add</param>
        /// <param name="scopeName">Optional scope qualifier</param>
        /// <returns></returns>
        ITemplateDefinition AddData(Type dataType, string scopeName = null);

        /// <summary>
        /// Finds a supplier of the specified type of data and adds it to the
        /// render context for the purpose of data binding
        /// </summary>
        /// <param name="scopeName">Optional scope qualifier</param>
        /// <returns></returns>
        ITemplateDefinition AddData<T>(string scopeName = null);

        /// <summary>
        /// Finds a supplier of the specified type of data and adds it to the
        /// render context for the purpose of data binding
        /// </summary>
        /// <param name="dataType">The type of data to add</param>
        /// <param name="propertyName">The name of the property to extract</param>
        /// <param name="scopeName">Optional scope to put the extracted data into</param>
        /// <param name="propertyScopeName">Optional scope to get the data from</param>
        /// <returns></returns>
        ITemplateDefinition ExtractProperty(Type dataType, string propertyName, string scopeName = null, string propertyScopeName = null);

        /// <summary>
        /// Finds a supplier of the specified type of data and adds it to the
        /// render context for the purpose of data binding
        /// </summary>
        /// <param name="propertyExpression">A lambda function identifying the property to extract</param>
        /// <param name="scopeName">Optional scope to put the extracted data into</param>
        /// <param name="propertyScopeName">Optional scope to get the data from</param>
        /// <returns></returns>
        ITemplateDefinition ExtractProperty<T>(Expression<Func<T, object>> propertyExpression, string scopeName = null, string propertyScopeName = null);

        /// <summary>
        /// Adds data from a property of a data bound object to the Html output
        /// by the template when it is rendered.
        /// </summary>
        /// <param name="dataType">The type of data to bind to. This will typically 
        /// be an object with public properties</param>
        /// <param name="propertyName">The name of the public property to read from 
        /// the data object</param>
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
        /// Adds data from a property of a data bound object to the Html output
        /// by the template when it is rendered.
        /// </summary>
        /// <param name="propertyExpression">A lambda expression that refers to the property
        /// to read from the bound data</param>
        /// <param name="dataFormatter">Optional data formatter that will take the 
        /// property value and convert it to text. If you do do not pass a formatter 
        /// then the ToString() method will be called on the property value</param>
        /// <param name="scopeName">Optional scope name to use when resolving
        /// data from the data context</param>
        ITemplateDefinition AddDataField<T>(
            Expression<Func<T, object>> propertyExpression,
            IDataFieldFormatter dataFormatter = null,
            string scopeName = null);

        /// <summary>
        /// Adds data from a property of a data bound object to the Html output
        /// by the template when it is rendered.
        /// </summary>
        /// <param name="formatFunc">A lambda expression that formats the data
        /// into Html</param>
        /// <param name="scopeName">Optional scope name to use when resolving
        /// data from the data context</param>
        ITemplateDefinition AddDataField<T>(Func<T, string> formatFunc, string scopeName = null);

        /// <summary>
        /// Adds a paragraph of static text that can be localized
        /// </summary>
        /// <param name="assetName">A unique name for this localizable asset. See
        /// Asset Manager for more information</param>
        /// <param name="defaultText">The default text to use for all unsupported
        /// localizations. Note that if you have not set up localization then this
        /// is the text that will always be used</param>
        /// <param name="isPreFormatted">Pass true to output text exactly as is 
        /// without adding indentation or collapsing multiple spaces</param>
        ITemplateDefinition AddText(string assetName, string defaultText, bool isPreFormatted = false);

        /// <summary>
        /// Adds HTML to the head section of the page
        /// </summary>
        ITemplateDefinition AddHeadLine(string html);

        /// <summary>
        /// Adds JavaScript to the head of the page
        /// </summary>
        ITemplateDefinition AddScriptLine(string javaScript);

        /// <summary>
        /// Adds CSS to the head of the page
        /// </summary>
        ITemplateDefinition AddStyleLine(string css);

        /// <summary>
        /// Adds JavaScript to the bottom of the page HTML
        /// </summary>
        ITemplateDefinition AddInitializationLine(string javaScript);

        /// <summary>
        /// Adds Javascript to a static asset that is referenced by any 
        /// page that renders this template
        /// </summary>
        /// <param name="rawJavascript">Raw unescaped Javascript</param>
        ITemplateDefinition AddStaticJavascript(string rawJavascript);

        /// <summary>
        /// Adds CSS to a static asset that is referenced by any page that 
        /// renders this template
        /// </summary>
        /// <param name="css">Raw unescaped CSS</param>
        ITemplateDefinition AddStaticCss(string css);

        /// <summary>
        /// Constructs and returns the template that was defined
        /// </summary>
        ITemplate Build();
    }
}
