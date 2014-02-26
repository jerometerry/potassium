namespace Sodium
{
    internal class SwitchBehaviorEvent<TA> : Event<TA>
    {
        private IListener listener;

        public SwitchBehaviorEvent(Behavior<Behavior<TA>> bba)
        {
            var callback = new SwitchBehaviorCallback<TA>(this);
            listener = bba.Value().Listen(callback, this.Rank);
        }
    }
}
