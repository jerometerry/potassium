namespace Sodium
{
    internal class SwitchBehaviorEvent<TA> : Event<TA>
    {
        private SwitchBehaviorAction<TA> action;

        public SwitchBehaviorEvent(Behavior<Behavior<TA>> bba)
        {
            this.action = new SwitchBehaviorAction<TA>(this);
            bba.Value().Listen(this.action, this.Rank);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.action != null)
                {
                    this.action.Dispose();
                    this.action = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
