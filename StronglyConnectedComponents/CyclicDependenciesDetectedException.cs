using System;
#if NETFRAMEWORK
using System.Runtime.Serialization;

#endif

namespace StronglyConnectedComponents
{
#if NETFRAMEWORK
  [Serializable]
#endif
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

#if NETFRAMEWORK
    protected CyclicDependenciesDetectedException(
      SerializationInfo info,
      StreamingContext context)
      : base(info, context)
    {
    }
#endif
  }
}