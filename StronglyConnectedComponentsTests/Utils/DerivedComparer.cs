using System;
using System.Collections.Generic;

namespace StronglyConnectedComponents.Tests.Utils
{
  public static class DerivedComparer
  {
    public static IEqualityComparer<TInput> Create<TInput, TCompared>(
      IEqualityComparer<TCompared> comparer, Func<TInput, TCompared> prepareValue)
    {
      return new DerivedComparer<TInput, TCompared>(comparer, prepareValue);
    }
  }

  public class DerivedComparer<TInput, TCompared> : IEqualityComparer<TInput>
  {
    private readonly Func<TInput, TCompared> _PrepareValue;
    public IEqualityComparer<TCompared> ActualComparer { get; }

    TCompared PrepareValue(TInput value)
    {
      if (value == null)
        return default(TCompared);

      return _PrepareValue(value);
    }

    public DerivedComparer(IEqualityComparer<TCompared> actualComparer, Func<TInput, TCompared> prepareValue)
    {
      _PrepareValue = prepareValue ?? throw new ArgumentNullException(nameof(prepareValue));
      ActualComparer = actualComparer ?? EqualityComparer<TCompared>.Default;
    }

    public bool Equals(TInput x, TInput y)
    {
      return ActualComparer.Equals(PrepareValue(x), PrepareValue(y));
    }

    public int GetHashCode(TInput obj)
    {
      return ActualComparer.GetHashCode(PrepareValue(obj));
    }
  }
}