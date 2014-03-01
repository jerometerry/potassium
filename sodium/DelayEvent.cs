namespace Sodium
{
    internal class DelayEvent<TA> : Event<TA>
    {
        private IEventListener<TA> listener;

        public DelayEvent(Event<TA> source)
        {
            var action = new SodiumAction<TA>((t, a) => t.Post(() => this.Fire(a)));
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
