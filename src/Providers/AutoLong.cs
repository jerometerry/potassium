namespace Potassium.Providers
{
    /// <summary>
    /// AutoLong is an IProvider of type long that starts with an initial value, 
    /// and auto increments by a step after each request of the Value
    /// </summary>
    public class AutoLong : IProvider<long>
    {
        private long value;
        private int increment;

        public AutoLong()
            : this(0, 1)
        {

        }

        public AutoLong(long value)
            : this(value, 1)
        {

        }

        public AutoLong(long value, int increment)
        {
            this.value = value;
            this.increment = increment;
        }

        public long Value
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
