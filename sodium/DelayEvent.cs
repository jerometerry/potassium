namespace Sodium
{
    internal class DelayEvent<TA> : Event<TA>
    {
        public DelayEvent(Event<TA> evt)
        {
            var callback = new Callback<TA>((t, a) => t.Post(() => this.Fire(a)));
            evt.Listen(callback, this.Rank);
        }
    }
}
