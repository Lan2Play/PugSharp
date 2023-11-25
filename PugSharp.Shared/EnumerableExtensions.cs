namespace PugSharp.Shared
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Randomize<T>(this IEnumerable<T> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return source.RandomizeInternal();
        }

        private static IEnumerable<T> RandomizeInternal<T>(
            this IEnumerable<T> source)
        {
            var buffer = source.ToList();
            for (int i = 0; i < buffer.Count; i++)
            {
                int j = Random.Shared.Next(i, buffer.Count);
                yield return buffer[j];

                buffer[j] = buffer[i];
            }
        }

        public static void AddRange<T>(this IList<T> items, IEnumerable<T> itemsToAdd)
        {
            foreach (var item in itemsToAdd)
            {
                items.Add(item);
            }
        }
    }
}
