namespace JT.Rx.Net.Internal
{
    

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
