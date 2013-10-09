using System.Collections;
using System.Collections.Generic;
using System.Linq;
using StronglyConnectedComponents.Core;

namespace StronglyConnectedComponents
{
  public sealed class DependencyCycle<T> : IEnumerable<T>
  {
    public bool IsCyclic { get; private set; }
    public ISet<T> Contents { get; private set; }
    public ISet<DependencyCycle<T>> Dependencies { get; private set; }

    public int Count
    {
      get { return Contents.Count; }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public IEnumerator<T> GetEnumerator()
    {
      return Contents.GetEnumerator();
    }

    public DependencyCycle(ISet<T> contents, ISet<DependencyCycle<T>> dependencies)
    {
      IsCyclic = contents.Count > 1;
      Contents = contents;
      Dependencies = dependencies;
    }
  }
}