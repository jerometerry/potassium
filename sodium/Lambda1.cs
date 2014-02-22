namespace Sodium
{
    using System;

    public sealed class Lambda1<TA, TB> : ILambda1<TA, TB>
    {
        private readonly Func<TA, TB> action;

        public Lambda1(Func<TA, TB> action)
        {
            this.action = action;
        }

        public TB Apply(TA a)
        {
            return action(a);
        }
    }
}