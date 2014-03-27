namespace Potassium.Providers
{
    /// <summary>
    /// IProvider is an interface that exposes a Property Value that provides values upon request.
    /// </summary>
    /// <typeparam name="T">The type of value provided</typeparam>
    public interface IProvider<out T>
    {
        /// <summary>
        /// Samples the current value
        /// </summary>
        T Value
        {
            get;
        }
    }
}
