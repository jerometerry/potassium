namespace Sodium
{
    using System;

    public class CosBehavior : IBehavior<Func<double, double>>
    {
        public Func<double, double> Value
        {
            get
            {
                return Math.Cos;
            }
        }
    }
}