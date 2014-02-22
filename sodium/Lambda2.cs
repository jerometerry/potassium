namespace Sodium
{
    using System;

    public sealed class Lambda2<TA, TB, TC> : ILambda2<TA, TB, TC>
    {
        private readonly Func<TA, TB, TC> f;

        public Lambda2(Func<TA, TB, TC> f)
        {
            this.f = f;
        }

        public TC Apply(TA a, TB b)
        {
            return f(a, b);
        }
    }
}