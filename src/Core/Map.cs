namespace JT.Rx.Net.Core
{
    using System;

    public class Map<TA,TB> : Monad<Func<TA, TB>>
    {
        private Func<TA, TB> map;

        public Map(Func<TA, TB> map)
        {
            this.map = map;
        }

        public override Func<TA, TB> Value
        {
            get
            {
                return this.map;
            }
        }
    }
}