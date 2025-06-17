using System.Reflection;
using NUnit.Framework;

public class FrameTests
{
    [Test]
    public void GetNeighborCount_ReturnsCorrectCount()
    {
        var frame = new Frame(3, 3);

        // Arrange a simple pattern around cell (1,1)
        frame.Grid[0, 0] = true;
        frame.Grid[1, 0] = true;
        frame.Grid[0, 1] = true;
        // other cells remain false

        // Access private static method via reflection
        var method = typeof(Frame).GetMethod("GetNeighborCount", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(method, "GetNeighborCount method not found");
        var result = (int)method.Invoke(null, new object[] { frame, 1, 1 });

        Assert.AreEqual(3, result);
    }
}
