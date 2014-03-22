namespace JT.Rx.Net.Core
{
    /// <summary>
    /// IProvider is an interface that exposes a Property Value that provides values upon request.
    /// </summary>
    /// <typeparam name="T">The type of value provided</typeparam>
    public interface IProvider<out T>
    {
        T Value
        {
            get;
        }
    }
}
