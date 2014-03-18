namespace Sodium
{
    public class BehaviorLoop<T> : Behavior<T>
    {
        public BehaviorLoop(T initValue)
            : base(new EventLoop<T>(), initValue)
        {
            this.Register(this.Source);
        }

        public void Loop(IObservable<T> source)
        {
            var loop = (EventLoop<T>)this.Source;
            loop.Loop(source);
        }
    }
}
