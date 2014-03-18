namespace Sodium
{
    internal sealed class SuppressedSubscribe<T> : EventLoop<T>
    {
        public SuppressedSubscribe(IObservable<T> source)
        {
            this.Loop(source);
        }

        protected override bool Refire(ISubscription<T> subscription, Transaction transaction)
        {
            return false;
        }
    }
}
