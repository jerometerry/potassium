namespace Sodium
{
    using System;

    public class ApplyBehavior<TA, TB> : Behavior<TB>
    {
        private Behavior<TA> source;
        private Behavior<Func<TA, TB>> bf;

        public ApplyBehavior(Behavior<TA> source, Behavior<Func<TA, TB>> bf)
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
