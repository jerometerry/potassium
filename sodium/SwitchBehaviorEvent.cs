namespace Sodium
{
    internal class SwitchBehaviorEvent<TA> : Event<TA>
    {
        private SwitchBehaviorCallback<TA> callback;

        public SwitchBehaviorEvent(Behavior<Behavior<TA>> bba)
        {
            callback = new SwitchBehaviorCallback<TA>(this);
            bba.Value().Listen(callback, this.Rank);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (callback != null)
                {
                    callback.Dispose();
                    callback = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
