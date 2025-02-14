namespace Yordi.Tools.Extensions
{
    public static class ExtensionsHelper
    {
        public static int Search(this byte[] array, byte[] buscaPor, int start = 0)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(start);
            if (buscaPor == null || buscaPor.Length == 0)
                throw new ArgumentNullException(nameof(buscaPor));
            if (array == null || array.Length == 0)
                return -1;
            int end = array.Length - buscaPor.Length; // past here no match is possible
            if (end < 0)
                throw new ArgumentOutOfRangeException($"byte[] {nameof(buscaPor)}  não pode ser maior que a origem");

            byte firstByte = buscaPor[0]; // cached to tell compiler there's no aliasing

            while (start <= end)
            {
                // scan for first byte only. compiler-friendly.
                if (array[start] == firstByte)
                {
                    // scan for rest of sequence
                    for (int offset = 1; ; ++offset)
                    {
                        if (offset == buscaPor.Length)
                        { // full sequence matched?
                            return start;
                        }
                        else if (array[start + offset] != buscaPor[offset])
                        {
                            break;
                        }
                    }
                }
                ++start;
            }

            // end of array reached without match
            return -1;
        }

        public static int FindIndex(this byte[] src, byte[] pattern, int start = 0, int? stop = null)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(start);
            if (pattern == null || pattern.Length == 0)
                throw new ArgumentNullException(nameof(pattern));
            if (src == null || src.Length == 0)
                return -1;
            int end = src.Length - pattern.Length; // past here no match is possible
            if (end < 0)
                throw new ArgumentOutOfRangeException($"byte[] {nameof(pattern)}  não pode ser maior que a origem");
            if (stop == null || stop.Value > (src.Length - pattern.Length))
                stop = src.Length - pattern.Length + 1;
            for (int i = start; i < stop; i++)
            {
                if (src[i] != pattern[0]) // compare only first byte
                    continue;

                // found a match on first byte, now try to match rest of the pattern
                for (int j = pattern.Length - 1; j >= 1; j--)
                {
                    if (src[i + j] != pattern[j]) break;
                    if (j == 1) return i;
                }
            }
            return -1;
        }

        public static IEnumerable<T> ReplaceAt<T>(this IEnumerable<T> collection, int index, T item)
        {
            var currentIndex = 0;
            foreach (var originalItem in collection)
            {
                if (currentIndex != index)
                {
                    //keep the original item in place
                    yield return originalItem;
                }
                else
                {
                    //we reached the index where we want to replace
                    yield return item;
                }
                currentIndex++;
            }
        }

        public static IEnumerable<T> ReplaceAt<T>(this IEnumerable<T> collection, T item, IEqualityComparer<T> comparer)
        {
            comparer = comparer ?? EqualityComparer<T>.Default;
            foreach (var originalItem in collection)
            {
                if (comparer.Equals(originalItem, item))
                    yield return item;
                else
                    yield return originalItem;
            }
        }

        /// <summary>
        /// Retorna se duas listas são iguais usando o comparador informado
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="l1"></param>
        /// <param name="l2"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public static bool Compare<T>(IEnumerable<T> l1, IEnumerable<T> l2, IEqualityComparer<T> comparer)
        {
            comparer ??= EqualityComparer<T>.Default;
            if (l1 == null || l2 == null) return false;
            if (l1.Count() != l2.Count()) return false;
            foreach (var item in l1)
            {
                var busca = l2.FirstOrDefault(m => comparer.Equals(m, item));
                if (busca == null) return false;
            }
            return true;

        }



        public static IEnumerable<T> Clone<T>(this IEnumerable<T> lista) where T : IClone<T>
        {
            if (lista == null || !lista.Any())
                yield break;
            foreach (var item in lista)
                yield return item.Clone();
        }

    }


}
