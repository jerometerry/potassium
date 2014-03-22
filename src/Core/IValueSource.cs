namespace JT.Rx.Net.Core
{
    public interface IValueSource<out T>
    {
        T Value
        {
            get;
        }
    }
}
