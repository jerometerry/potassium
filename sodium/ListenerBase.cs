namespace sodium
{
    public abstract class ListenerBase : IListener
    {
        public virtual void unlisten() { }

        ///
        /// Combine listeners into one where a single unlisten() invocation will unlisten
        /// both the inputs.
        ///
        public IListener append(IListener listener)
        {
            return new DualListener(this, listener);
        }
    }
}