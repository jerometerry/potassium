namespace Sodium
{
    internal class DelayEvent<TA> : Event<TA>
    {
        public DelayEvent(Event<TA> evt)
        {
            var action = new SodiumAction<TA>((t, a) => t.Post(() => this.Fire(a)));
            evt.Listen(action, this.Rank);
        }
    }
}
