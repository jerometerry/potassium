namespace Sodium
{
    internal class DelayEvent<TA> : Event<TA>
    {
        IListener listener;
        Event<TA> evt;

        public DelayEvent(Event<TA> evt)
        {
            var callback = new Callback<TA>((t, a) => t.Post(() => this.Fire(a)));
            this.evt = evt;
            listener = evt.Listen(callback, this.Rank);
        }
    }
}
