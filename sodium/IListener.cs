namespace sodium
{
    public interface IListener
    {
        void Unlisten();

        ///
        /// Combine listeners into one where a single unlisten() invocation will unlisten
        /// both the inputs.
        ///
        IListener Append(IListener listener);
    }
}