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

        public override void Close()
        {
            if (listener != null)
            {
                listener.Close();
                listener = null;
            }

            base.Close();
        }
    }
}
