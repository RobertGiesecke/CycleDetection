using System.Collections.Generic;
using System.Linq;

namespace StronglyConnectedComponents.Tests
{
  public class TestBase
  {
    public static string Join(string separator, IEnumerable<string> list)
    {
      return string.Join(separator, list.ToArray());
    }
  }
}