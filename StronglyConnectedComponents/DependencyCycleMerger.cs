#if NETFRAMEWORK || (!NETSTANDARD1_0 && !NETSTANDARD1_1)
#define FEATURE_ConvertAll
#endif

using System;
using System.Collections.Generic;
using System.Linq;

namespace StronglyConnectedComponents
{
  public static class DependencyCycleMerger
  {
    public delegate T MergeCycleHandler<T>(DependencyCycle<T> cycle, Func<DependencyCycle<T>, T> getMerged);

    #if NET40
    internal static IList<T> AsListInternal<T>(this IEnumerable<T> enumerable)
    {
      return enumerable as IList<T> ?? enumerable.ToList();
    }
    #else
    internal static IReadOnlyList<T> AsListInternal<T>(this IEnumerable<T> enumerable)
    {
      return enumerable as IReadOnlyList<T> ?? enumerable.ToList();
    }
    #endif

    internal static List<TResult> ConvertAllInternal<T, TResult>(this IEnumerable<T> enumerable, Func<T, TResult> converter)
    {
#if FEATURE_ConvertAll
      if (enumerable is List<T> l)
      {
        return l.ConvertAll(converter.Invoke);
      }
#endif

      var sourceList = enumerable.AsListInternal();
      var result = new List<TResult>(sourceList.Count);
      result.AddRange(sourceList.Select(converter));
      return result;
    }

    /// <summary>
    /// Allows to merge cyclic dependencies into a single instance of the source element type.
    /// </summary>
    /// <param name="components">Required. A lsequenceist of dependency components as returned by <see cref="DetectCycles{T}"/>.</param>
    /// <param name="mergeCycle" >Not required. A delegate that take the cyclic component to merge and returns the merged instance of <see cref="T"/>.
    /// Will throw a <see cref="CyclicDependenciesDetectedException"/> when a cycle is found but <see cref="mergeCycle"/> is null.</param>
    public static IEnumerable<T> MergeCyclicDependencies<T>(this IEnumerable<DependencyCycle<T>> components,
      MergeCycleHandler<T> mergeCycle = null)
    {
      var asList = components.AsListInternal();

      if (!asList.Any(t => t.IsCyclic))
        return asList.ConvertAllInternal(t => t.Contents.Single());

      if (mergeCycle == null)
        mergeCycle = delegate
        {
          throw new CyclicDependenciesDetectedException(
            "A cyclic dependency has been detected. This method cannot continue without a value for mergeCycle.");
        };

      var mergedValues = new Dictionary<DependencyCycle<T>, T>();

      asList.VisitCycles((cycle, changed, dependencies) =>
      {
        var hasCycles = false;
        Func<bool> needsToRun = () =>
        {
          cycle.Dependencies.VisitCycles((c, x, set) =>
          {
            if (c.IsCyclic)
            {
              hasCycles = true;
              return null;
            }

            return c;
          }).Count();

          return hasCycles;
        };

        if (!cycle.IsCyclic && !needsToRun())
        {
          return cycle;
        }

        T result;

        if (mergedValues.TryGetValue(cycle, out result))
          return cycle;

        Func<DependencyCycle<T>, T> getMerged = c =>
        {
          T r;
          mergedValues.TryGetValue(c, out r);
          return r;
        };

        if (changed || hasCycles)
        {
          var newCycle = new DependencyCycle<T>(cycle.Contents, dependencies);
          mergedValues.Add(newCycle, result = mergeCycle(newCycle, getMerged));
          mergedValues[cycle] = result;
          return newCycle;
        }

        mergedValues.Add(cycle, mergeCycle(cycle, getMerged));
        return cycle;
      }).Count();

      return asList.ConvertAllInternal(c =>
      {
        T merged;
        if (mergedValues.TryGetValue(c, out merged))
          return merged;

        return c.Contents.Single();
      });
    }

    internal static HashSet<T> ToHashSet<T>(this IEnumerable<T> list, IEqualityComparer<T> comparer = null)
    {
      return comparer != null
        ? new HashSet<T>(list, comparer)
        : new HashSet<T>(list);
    }
  }
}
