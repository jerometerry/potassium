namespace Sodium
{
    internal class SwitchBehaviorEvent<TA> : Event<TA>
    {
        private SwitchBehaviorAction<TA> action;
        private IEventListener<Behavior<TA>> listener;

        public SwitchBehaviorEvent(Behavior<Behavior<TA>> bba)
        {
            this.action = new SwitchBehaviorAction<TA>(this);
            this.listener = bba.Value().Listen(this.action, this.Rank);
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

                if (this.listener != null)
                {
                    this.listener.Dispose();
                    this.listener = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
