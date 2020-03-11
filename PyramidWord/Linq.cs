using System;
using System.Collections.Generic;

namespace PyramidWord
{
    public static class Linq
    {
        // The framework provided All Linq extension should work fine but doesn't support
        // index and although we know its sequential and could work with a closure we don't
        // want to rely on implementation detail.
        public static bool All<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
        {
            if (source == null)
                throw new  ArgumentNullException(nameof(source));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            var index = 0;
            foreach (TSource source1 in source)
            {
                if (!predicate(source1, index++))
                    return false;
            }
            return true;
        }
    }
}
