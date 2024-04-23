namespace EFCoreExtras;

public static class ModelSelection
{
    public static List<List<T>> SplitIntoBatches<T>(List<T> objects, int batchSize)
        where T : class
    {
        var batches = new List<List<T>>();
        for (int i = 0; i < objects.Count; i += batchSize)
        {
            List<T> batch = objects.Skip(i).Take(batchSize).ToList();
            batches.Add(batch);
        }
        return batches;
    }
}