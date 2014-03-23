namespace Potassium.Providers
{
    public class AutoDouble : IProvider<double>
    {
        private double value;
        private double increment;

        public AutoDouble()
            : this(0.0, 1.0)
        {

        }

        public AutoDouble(double value)
            : this(value, 1.0)
        {

        }

        public AutoDouble(double value, double increment)
        {
            this.value = value;
            this.increment = increment;
        }

        public double Value
        {
            get
            {
                var result = value;
                value += increment;
                return result;
            }
        }
    }
}
