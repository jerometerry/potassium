namespace Sodium
{
    internal sealed class SuppressedSubscribe<T> : EventLoop<T>
    {
        public SuppressedSubscribe(IEvent<T> source)
        {
            this.Loop(source);
        }

        internal override bool Refire(ISubscription<T> subscription, Transaction transaction)
        {
            return false;
        }
    }
}
