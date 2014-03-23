namespace Potassium.Providers
{
    public class AutoInt : IProvider<int>
    {
        private int value;
        private int increment;

        public AutoInt()
            : this(0, 1)
        {

        }

        public AutoInt(int value)
            : this(value, 1)
        {

        }

        public AutoInt(int value, int increment)
        {
            this.value = value;
            this.increment = increment;
        }

        public int Value
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
