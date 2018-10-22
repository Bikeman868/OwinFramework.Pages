namespace OwinFramework.Pages.Html.Templates.Text
{
    /// <summary>
    /// This interface is implemented by document elements that are containers 
    /// for other elements where they arrange the children according to the 
    /// ContainerType property
    /// </summary>
    public interface IContainerElement
    {
        /// <summary>
        /// The type of container
        /// </summary>
        ContainerTypes ContainerType { get; set; }

        /// <summary>
        /// Contains information about how to arrange the children
        /// within the container. The interpretation of this property
        /// value is specific to the container type
        /// </summary>
        object ChildLayout { get; set; }
    }
}