namespace Sodium
{
    using System;

    public class EventLoop<TA> : Event<TA>
    {
        private Event<TA> _eaOut;

        public void Loop(Event<TA> eaOut)
        {
            if (_eaOut != null)
            { 
                throw new ApplicationException("EventLoop looped more than once");
            }

            _eaOut = eaOut;
            var me = this;
            RegisterListener(eaOut.Listen(Node, new TransactionHandler<TA>(me.Send)));
        }
    }
}