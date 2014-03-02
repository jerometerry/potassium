namespace Sodium
{
    internal class DelayEvent<T> : Event<T>
    {
        private IEventListener<T> listener;

        public DelayEvent(Event<T> source)
        {
            var action = new SodiumCallback<T>((t, a) => t.Post(() => this.Fire(a)));
            this.listener = source.Listen(action, this.Rank);
        }

        public override void Dispose()
        {
            if (listener != null)
            {
                listener.Dispose();
                listener = null;
            }

            base.Dispose();
        }
    }
}
