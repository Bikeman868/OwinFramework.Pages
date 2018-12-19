namespace OwinFramework.Pages.Restful.Parameters
{
    /// <summary>
    /// Validates that a service endpoint is of a specific type but
    /// allows any value
    /// </summary>
    public class AnyValue<T>: ParameterParser
    {
        public AnyValue(): base(typeof(T))
        { }
    }
}
