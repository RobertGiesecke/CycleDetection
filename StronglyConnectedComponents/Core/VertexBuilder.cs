﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace StronglyConnectedComponents.Core
{
  public static class VertexBuilder
  {
    /// <summary>
    /// Builds a sequence of <see cref="Vertex{T}">dependancy vertices</see> from a sequence of source elements.
    /// </summary>
    /// <param name="source">Required. The sequence of source elements that need to be converted into vertices.</param>
    /// <param name="dependencySelector">Required. A delegate that takes an items of <see cref="source"/> and returns a sequence of dependencies.
    /// <remarks>Can return null to indicate no dependency.</remarks></param>
    /// <param name="comparer">Required. An implementation of <see cref="IEqualityComparer{T}"/>, this is used to compare the values with their dependencies.</param>
    public static IEnumerable<Vertex<T>> BuildVertices<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> dependencySelector, IEqualityComparer<T> comparer)
    {
      if (source == null) throw new ArgumentNullException("source");
      if (dependencySelector == null) throw new ArgumentNullException("getDependencies");
      if (comparer == null) throw new ArgumentNullException("comparer");

      var vertexBySource = new ConcurrentDictionary<T, Vertex<T>>(comparer);

      return source.Select(t => BuildVertex(vertexBySource, t, dependencySelector));
    }

    private static Vertex<T> BuildVertex<T>(ConcurrentDictionary<T, Vertex<T>> vertexBySource, T source, Func<T, IEnumerable<T>> dependencySelector)
    {
      Vertex<T> vertex;
      if (vertexBySource.TryGetValue(source, out vertex))
        return vertex;

      if (vertexBySource.TryAdd(source, vertex = new Vertex<T>(source)))
      {
        var dependencies = dependencySelector(vertex.Value);

        if (dependencies != null)
          foreach (var d in dependencies)
            vertex.Dependencies.Add(BuildVertex(vertexBySource, d, dependencySelector));
        return vertex;
      }

      return vertexBySource[source];
    }
  }
}