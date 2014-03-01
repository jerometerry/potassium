namespace Sodium
{
    internal class DelayEvent<TA> : Event<TA>
    {
        private IEventListener<TA> listener;

        public DelayEvent(Event<TA> evt)
        {
            var action = new SodiumAction<TA>((t, a) => t.Post(() => this.Fire(a)));
            this.listener = evt.Listen(action, this.Rank);
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
