namespace Sodium.Discrete
{
    using Sodium.Core;

    internal sealed class SuppressedSubscribeEvent<T> : EventFeed<T>
    {
        public SuppressedSubscribeEvent(Observable<T> source)
        {
            this.Feed(source);
        }

        protected override bool Republish(ISubscription<T> subscription, Transaction transaction)
        {
            return false;
        }
    }
}
