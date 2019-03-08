using System;
using System.Collections.Generic;
using System.Linq;

namespace StronglyConnectedComponents.Tests.Utils
{
  public class TestValue<T>
  {
    public T Value { get; }
    public IList<T> DependsOn { get; }

    public TestValue(T value, params T[] dependsOn)
    {
      Value = value;
      DependsOn = dependsOn.Distinct().ToList().AsReadOnly();
    }

    public override string ToString()
    {
      return String.Format("Value: {0}, DependsOn: {1}", Value, String.Join(", ", (DependsOn ?? new T[0]).ToArray()));
    }

    public static readonly IEqualityComparer<TestValue<T>> ValueComparer = TestValue.CreateComparer<T>();
  }

  public static class TestValue
  {
    public static TestValue<T> Create<T>(T value, params T[] dependsOn)
    {
      return new TestValue<T>(value, dependsOn);
    }

    public static IEqualityComparer<TestValue<T>> CreateComparer<T>(IEqualityComparer<T> valueComparer = null)
    {
      return new ValueEqualityComparer<T>(valueComparer);
    }

    private sealed class ValueEqualityComparer<T> : IEqualityComparer<TestValue<T>>
    {
      private readonly IEqualityComparer<T> _Comparer;

      public ValueEqualityComparer(IEqualityComparer<T> comparer)
      {
        _Comparer = comparer ?? EqualityComparer<T>.Default;
      }

      public bool Equals(TestValue<T> x, TestValue<T> y)
      {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return _Comparer.Equals(x.Value, y.Value);
      }

      public int GetHashCode(TestValue<T> obj)
      {
        return _Comparer.GetHashCode(obj.Value);
      }
    }
  }
}