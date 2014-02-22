using System;

namespace Sodium
{
    public class Handler<TA> : IHandler<TA>
    {
        private readonly Action<TA> _action;

        public Handler(Action<TA> action)
        {
            _action = action;
        }

        public void Run(TA a)
        {
            _action(a);
        }
    }
}