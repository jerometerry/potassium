namespace Sodium
{
    using System;

    public sealed class EventLoop<TA> : Event<TA>
    {
        private Event<TA> evt;

        public void Loop(Event<TA> e)
        {
            if (evt != null)
            { 
                throw new ApplicationException("EventLoop looped more than once");
            }

            evt = e;
            var loop = this;
            RegisterListener(e.Listen(this.Node, new TransactionHandler<TA>(loop.Send)));
        }
    }
}