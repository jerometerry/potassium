namespace Sodium
{
    public interface IListener
    {
        void Unlisten();

        /// <summary>
        /// Combine listeners into one where a single unlisten() invocation will unlisten
        /// both the inputs.
        /// </summary>
        IListener Append(IListener listener);
    }
}