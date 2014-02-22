namespace Sodium
{
    using System;

    public sealed class Lambda3<TA, TB, TC, TD> : ILambda3<TA, TB, TC, TD>
    {
        private readonly Func<TA, TB, TC, TD> f;

        public Lambda3(Func<TA, TB, TC, TD> f)
        {
            this.f = f;
        }

        public TD Apply(TA a, TB b, TC c)
        {
            return f(a, b, c);
        }
    }
}