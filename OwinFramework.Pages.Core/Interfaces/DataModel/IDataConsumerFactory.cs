namespace OwinFramework.Pages.Core.Interfaces.DataModel
{
    /// <summary>
    /// Factory for data consumers
    /// </summary>
    public interface IDataConsumerFactory
    {
        /// <summary>
        /// Creates and initial;izes a new data consumer
        /// </summary>
        IDataConsumer Create();
    }
}
