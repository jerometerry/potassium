namespace Sodium
{
    internal sealed class SuppressedSubscribeEvent<T> : EventLoop<T>
    {
        public SuppressedSubscribeEvent(Observable<T> source)
        {
            this.Loop(source);
        }

        protected override bool Republish(ISubscription<T> subscription, Transaction transaction)
        {
            return false;
        }
    }
}
