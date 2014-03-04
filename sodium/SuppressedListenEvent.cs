namespace Sodium
{
    internal class SuppressedListenEvent<T> : EventLoop<T>
    {
        public SuppressedListenEvent(Event<T> source)
        {
            this.Loop(source);
        }

        internal override bool Refire(IEventListener<T> listener, Transaction transaction)
        {
            return false;
        }
    }
}
