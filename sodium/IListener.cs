namespace sodium
{
    public interface IListener
    {
        void unlisten();

        ///
        /// Combine listeners into one where a single unlisten() invocation will unlisten
        /// both the inputs.
        ///
        IListener append(IListener listener);
    }
}