using System;

namespace sodium
{
    public class Lambda1<TA, TB> : ILambda1<TA, TB>
    {
        private readonly Func<TA, TB> _action;

        public Lambda1(Func<TA, TB> action)
        {
            _action = action;
        }

        public TB Apply(TA a)
        {
            return _action(a);
        }
    }
}