using System.Collections.Generic;

namespace StronglyConnectedComponents.Core
{
  /// <summary>
  /// Handles equality of <see cref="Vertex{T}"/> instances using their <see cref="Vertex{T}.Value"/> property.
  /// </summary>
  public sealed class VertexValueComparer<T> : IEqualityComparer<Vertex<T>>
  {
    private readonly IEqualityComparer<T> _Comparer;

    public VertexValueComparer(IEqualityComparer<T> comparer)
    {
      _Comparer = comparer;
    }

    public bool Equals(Vertex<T> x, Vertex<T> y)
    {
      if ((x == null) != (y == null))
        return false;
      if (x == null)
        return true;
      return _Comparer.Equals(x.Value, y.Value);
    }

    public int GetHashCode(Vertex<T> obj)
    {
      if (obj == null)
        return 0;

      return _Comparer.GetHashCode(obj.Value);
    }
  }
}