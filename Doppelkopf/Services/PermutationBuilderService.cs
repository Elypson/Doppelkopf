using System;
using System.Collections.Generic;
using System.Linq;

namespace Doppelkopf.Services
{
    public static class PermutationBuilderService
    {
        // for all possible permutations
        public static IEnumerable<IEnumerable<T>> GetKCombs<T>(IEnumerable<T> list, int length) where T : IComparable
        {
            if (length == 1)
            {
                return list.Select(t => new T[] { t });
            }

            return GetKCombs(list, length - 1)
                .SelectMany(t => list.Where(o => o.CompareTo(t.Last()) > 0),
                    (t1, t2) => t1.Concat(new T[] { t2 }));
        }

        // for a shifting window of (playerNumber - 4); all players are the first player exactly once
        public static IEnumerable<IEnumerable<T>> GetShiftingWindowCombinations<T>(IEnumerable<T> list, int length) where T : IComparable
        {
            IEnumerable<T> twiceList = list.Concat(list);
            
            List<IEnumerable<T>> result = new List<IEnumerable<T>>();
            for(int start = 0; start < list.Count(); ++start)
            {
                result.Add(twiceList.Skip(start).Take(length));
            }
            return result;
        }
    }
}
