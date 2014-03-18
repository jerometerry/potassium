namespace Sodium
{
    public class BehaviorSink<T> : Behavior<T>
    {
        public BehaviorSink(T initValue)
            : base(new EventSink<T>(), initValue)
        {
            this.Register(this.Source);
        }

        public bool Fire(T firing)
        {
            var sink = (EventSink<T>)this.Source;
            return sink.Fire(firing);
        }
    }
}
