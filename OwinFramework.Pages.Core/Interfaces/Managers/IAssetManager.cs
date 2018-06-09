namespace OwinFramework.Pages.Core.Interfaces.Managers
{
    /// <summary>
    /// The asset manager manages all JaavScript, css, images and text
    /// and provides localization of those things
    /// </summary>
    public interface IAssetManager
    {
        /// <summary>
        /// Specifies the default language to use when the asset manager
        /// does not support any of the languages accepted by the browser
        /// </summary>
        string DefaultLanguage { get; set; }

        /// <summary>
        /// Examines the browser accepted languages and selects the
        /// language for output rendering
        /// </summary>
        /// <param name="acceptLanguageHeader"></param>
        string GetSupportedLanguage(string acceptLanguageHeader);
    }
}
