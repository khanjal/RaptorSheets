namespace RaptorLoggerEngine.Tests.Data.Helpers;

public static class RandomHelpers
{

    public static int[] GetRandomOrder(int start, int length)
    {
        var array = Enumerable.Range(start, length).ToArray();

        var rng = new Random();
        return [.. array.OrderBy((item) => rng.NextDouble())];
    }

    internal static IList<IList<object>> RandomizeValues(IList<IList<object>> values, int[] valueOrder)
    {
        var objectList = new List<IList<object>>();

        foreach (var value in values)
        {
            var objectValues = new List<object>();

            for (int i = 0; i < valueOrder.Length; i++)
            {
                if (value.Count > valueOrder[i])
                {
                    objectValues.Add(value[valueOrder[i]]);
                }
                else
                {
                    objectValues.Add(" ");
                }
            }
            objectList.Add(objectValues);
        }
        return objectList;
    }
}
