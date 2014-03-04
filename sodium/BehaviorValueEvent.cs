namespace Sodium
{
    internal sealed class BehaviorValueEvent<T> : EventSink<T>
    {
        private Behavior<T> behavior;
        private IEventListener<T> listener;

        public BehaviorValueEvent(Behavior<T> behavior, ActionScheduler scheduler)
        {
            this.behavior = behavior;
            var callback = this.CreateFireCallback();
            listener = behavior.Listen(callback, this.Rank, scheduler);
        }

        protected internal override T[] InitialFirings()
        {
            return new[] { behavior.Value };
        }

        protected override void Dispose(bool disposing)
        {
            if (listener != null)
            {
                listener.Dispose();
                listener = null;
            }

            behavior = null;

            base.Dispose(disposing);
        }
    }
}