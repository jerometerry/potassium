namespace Sodium
{
    internal class SuppressedListenEvent<T> : EventLoop<T>
    {
        public SuppressedListenEvent(Event<T> source)
        {
            this.Loop(source);
        }

        protected override bool Refire(IEventListener<T> listener, ActionScheduler scheduler)
        {
            return false;
        }
    }
}
