﻿namespace Sodium
{
    internal class DelayEvent<TA> : Event<TA>
    {
        private IEventListener<TA> listener;

        public DelayEvent(Event<TA> evt, bool allowAutoDispose)
            : base(allowAutoDispose)
        {
            var action = new SodiumAction<TA>((t, a) => t.Post(() => this.Fire(a)));
            this.listener = evt.Listen(action, this.Rank, true);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (listener != null)
                {
                    listener.AutoDispose();
                    listener = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
