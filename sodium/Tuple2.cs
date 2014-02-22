namespace Sodium
{
    public sealed class Tuple2<TA, TB>
    {
        private readonly TA v1;
        private readonly TB v2;

        public Tuple2(TA v1, TB v2)
        {
            this.v1 = v1;
            this.v2 = v2;
        }

        public TA V1
        {
            get
            {
                return v1;
            }
        }

        public TB V2
        {
            get
            {
                return v2;
            }
        }
    }
}