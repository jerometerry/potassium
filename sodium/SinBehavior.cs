namespace Sodium
{
    using System;

    public class SinBehavior : IBehavior<Func<double, double>>
    {
        public Func<double, double> Value
        {
            get
            {
                return Math.Sin;
            }
        }
    }
}