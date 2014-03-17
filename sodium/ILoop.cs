namespace Sodium
{
    public interface ILoop<T>
    {
        void Loop(IObservable<T> observable);
    }
}
