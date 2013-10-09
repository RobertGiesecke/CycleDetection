﻿using System;
using System.Collections.Generic;
using System.Linq;
using StronglyConnectedComponents.Core;

namespace StronglyConnectedComponents
{
  public static class DependencyResolver
  {
    /// <summary>
    /// Allows to merge cyclic dependencies into a single instance of the source element type.
    /// </summary>
    /// <param name="components">Required. A lsequenceist of dependency components as returned by <see cref="DetectCycles{T}"/>.</param>
    /// <param name="mergeCycle" >Not required. A delegate that take the cyclic component to merge and returns the merged instance of <see cref="T"/>.
    /// Will throw a <see cref="InvalidOperationException"/> when a cycle is found but <see cref="mergeCycle"/> is null.</param>
    public static IEnumerable<T> FlattenCyclicDependencies<T>(this IEnumerable<DependencyCycle<T>> components, Func<DependencyCycle<T>, T> mergeCycle = null)
    {
      if (mergeCycle == null)
        mergeCycle = set => { throw new InvalidOperationException(string.Format("A cyclic dependency has been detected. This method cannot continue without a value for mergeCycle.")); };

      return from c in components
             select c.IsCyclic ? mergeCycle(c) : c.Contents.Single();
    }

    /// <summary>
    /// Sorts all items from <see cref="source"/> so that dependencies come first. It will group cyclic dependencies into a single component.
    /// </summary>
    /// <param name="source">Required. The sequence of elements that need to be sorted.</param>
    /// <param name="dependencySelector">Required. A delegate that takes an items of <see cref="source"/> and returns a sequence of dependencies.
    /// <remarks>Can return null to indicate no dependency.</remarks></param>
    /// <param name="comparer">Not required. A n implementation of <see cref="IEqualityComparer{T}"/>, this is used to compare the values with their dependencies.</param>
    public static IList<DependencyCycle<T>> DetectCycles<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> dependencySelector, IEqualityComparer<T> comparer = null)
    {
      var valueComparer = comparer ?? EqualityComparer<T>.Default;

      var components = ResolveStronglyConnectedComponents(source, dependencySelector, valueComparer);

      return ExtractCycles(components, valueComparer);
    }

    public static IList<DependencyCycle<T>> ExtractCycles<T>(this IEnumerable<StronglyConnectedComponent<T>> components, IEqualityComparer<T> valueComparer)
    {
      var asList = components.ToList();

      var getDependencyCycleFromSc = BuildGetDependencyCycleFromStronglyConnectedComponent(valueComparer, asList);

      return asList.ConvertAll(t => getDependencyCycleFromSc(t));
    }

    private static Func<StronglyConnectedComponent<T>, DependencyCycle<T>> BuildGetDependencyCycleFromStronglyConnectedComponent<T>(IEqualityComparer<T> valueComparer, IEnumerable<StronglyConnectedComponent<T>> asList)
    {
      var vertexComparer = new VertexValueComparer<T>(valueComparer);
      var byVertex = (from c in asList
                      from v in c
                      select new { c, v })
        .GroupBy(k => k.v, v => v.c, vertexComparer)
        .ToDictionary(k => k.Key, v => v.ToHashSet());

      var cycleByComponent = new Dictionary<StronglyConnectedComponent<T>, DependencyCycle<T>>();

      Func<StronglyConnectedComponent<T>, DependencyCycle<T>> getCycle = null;

      getCycle = sc =>
      {
        DependencyCycle<T> result;
        if (cycleByComponent.TryGetValue(sc, out result))
          return result;

        var dependencies = sc.SelectMany(t => t.Dependencies).SelectMany(d => byVertex[d]);
        var sets = new HashSet<DependencyCycle<T>>();
        DependencyCycle<T> cycle;
        cycleByComponent.Add(sc, cycle = new DependencyCycle<T>(sc.Select(t => t.Value).ToHashSet(valueComparer), sets));
        sets.UnionWith(dependencies.Select(getCycle).Where(t => t != cycle));
        return cycle;
      };

      return getCycle;
    }

    /// <summary>
    /// This is a wrapper around <see cref="StronglyConnectedComponentFinder{T}.DetectCycle"/> that uses the same way to setup dependencies as <see cref="DetectCycles{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source">Required. The sequence of elements that need to be sorted.</param>
    /// <param name="dependencySelector">Required. A delegate that takes an items of <see cref="source"/> and returns a sequence of dependencies.
    /// <remarks>Can return null to indicate no dependency.</remarks></param>
    /// <param name="comparer">Not required. A n implementation of <see cref="IEqualityComparer{T}"/>, this is used to compare the values with their dependencies.</param>
    public static StronglyConnectedComponentList<T> ResolveStronglyConnectedComponents<T>(
      IEnumerable<T> source, Func<T, IEnumerable<T>> dependencySelector, IEqualityComparer<T> comparer = null)
    {
      var valueComparer = comparer ?? EqualityComparer<T>.Default;
      var vertexComparer = new VertexValueComparer<T>(valueComparer);

      var finder = new StronglyConnectedComponentFinder<T>();

      var vertices = VertexBuilder.BuildVertices(source, dependencySelector, valueComparer);

      return finder.DetectCycle(vertices, vertexComparer);
    }

    /// <summary>
    /// Returns all components that are not part of a cyclic dependency.
    /// </summary>
    /// <param name="components">Required. A sequence of dependency components as returned by <see cref="DetectCycles{T}"/>.</param>
    /// <returns></returns>
    public static IEnumerable<DependencyCycle<T>> IndependentComponents<T>(this IEnumerable<DependencyCycle<T>> components)
    {
      if (components == null)
        throw new ArgumentNullException("components");
      return components.Where(c => !c.IsCyclic);
    }

    /// <summary>
    /// Returns only components that represent a cyclic dependancy.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="components">Required. A sequence of dependency components as returned by <see cref="DetectCycles{T}"/>.</param>
    /// <returns></returns>
    public static IEnumerable<DependencyCycle<T>> Cycles<T>(this IEnumerable<DependencyCycle<T>> components)
    {
      if (components == null)
        throw new ArgumentNullException("components");
      return components.Where(c => c.IsCyclic);
    }
  }
}