namespace EFCoreExtras;

public static class ModelSelection
{
    public static T[][] SplitIntoBatches<T>(IEnumerable<T> objects, int batchSize)
        where T : class
    {
        var len = objects.Count();
        var batches = new List<T[]>((len / batchSize) + 1);
        for (int i = 0; i < len; i += batchSize)
        {
            T[] batch = objects.Skip(i).Take(batchSize).ToArray();
            batches.Add(batch);
        }
        return [..batches];
    }

    public static T[][] SplitIntoBatches<T>(T[] objects, int batchSize)
        where T : class
    {
        var len = objects.Length;
        var batches = new List<T[]>((len / batchSize) + 1);
        for (int i = 0; i < len; i += batchSize)
        {
            T[] batch = objects.Skip(i).Take(batchSize).ToArray();
            batches.Add(batch);
        }
        return [.. batches];
    }
}
