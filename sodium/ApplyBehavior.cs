namespace Sodium
{
    using System;

    public class ApplyBehavior<T, TB> : ContinuousBehavior<TB>
    {
        private IBehavior<T> source;
        private IBehavior<Func<T, TB>> bf;

        public ApplyBehavior(IBehavior<T> source, IBehavior<Func<T, TB>> bf)
        {
            this.source = source;
            this.bf = bf;
        }

        public override TB Value
        {
            get
            {
                var map = bf.Value;
                var a = source.Value;
                var b = map(a);
                return b;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.source = null;
                this.bf = null;
            }

            base.Dispose(disposing);
        }
    }
}
