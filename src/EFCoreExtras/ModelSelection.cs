namespace EFCoreExtras;

public static class ModelSelection
{
    public static T[][] SplitIntoBatches<T>(IEnumerable<T> objects, int batchSize)
        where T : class
    {
        objects = objects is Array ? objects : objects.ToArray();

        var batches = new List<T[]>();
        var len = objects.Count();
        for (int i = 0; i < len; i += batchSize)
        {
            T[] batch = objects.Skip(i).Take(batchSize).ToArray();
            batches.Add(batch);
        }
        return [..batches];
    }
}
