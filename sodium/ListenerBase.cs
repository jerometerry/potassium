namespace Sodium
{
    public abstract class ListenerBase : IListener
    {
        public virtual void Unlisten() 
        { 
        }

        ///
        /// Combine listeners into one where a single unlisten() invocation will unlisten
        /// both the inputs.
        ///
        public IListener Append(IListener listener)
        {
            return new DualListener(this, listener);
        }
    }
}