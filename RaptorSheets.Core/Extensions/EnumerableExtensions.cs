namespace RaptorSheets.Core.Extensions;

public static class EnumerableExtensions
{
    public static void AddRange<T>(this IList<T> collection, IEnumerable<T> items)
    {
        if (collection == null)
        {
            throw new ArgumentNullException(nameof(collection));
        }

        if (items == null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        foreach (var item in items)
        {
            collection.Add(item);
        }
    }
}
