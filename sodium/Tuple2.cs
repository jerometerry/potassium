namespace sodium
{
    public sealed class Tuple2<A, B>
    {
        public Tuple2(A v1, B v2)
        {
            V1 = v1;
            V2 = v2;
        }
        public A V1;
        public B V2;
    }
}