using System;
using System.Runtime.Serialization;

namespace StronglyConnectedComponents
{
  [Serializable]
  public class CyclicDependenciesDetectedException : Exception
  {
    public CyclicDependenciesDetectedException()
    {
    }

    public CyclicDependenciesDetectedException(string message)
      : base(message)
    {
    }

    public CyclicDependenciesDetectedException(string message, Exception inner)
      : base(message, inner)
    {
    }

    protected CyclicDependenciesDetectedException(
      SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}