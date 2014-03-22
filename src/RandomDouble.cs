namespace JT.Rx.Net
{
    using System;
    

    public class RandomDouble : Monad<double>
    {
        private Random rnd = new Random();
        private double max;

        public RandomDouble(double max)
        {
            this.max = max;
        }

        public override double Value
        {
            get
            {
                return rnd.NextDouble() * max;
            }
        }
    }
}
