namespace PackageBuilder
{
    public interface ITempCache
    {
        object Get(string key);
        object Peek(string key);
        string Store(object value);
    }
}