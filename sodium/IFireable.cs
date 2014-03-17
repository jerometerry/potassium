namespace Sodium
{
    public interface IFireable<T>
    {
        bool Fire(T firing);
    }
}
