namespace BlazorHelloWorld.Tests;

[TestClass]
public sealed class Test1
{
    [TestMethod]
    [DataRow(1, 2, 3)]
    public void TestMethod1(int a, int b, int expected)
    {
        Assert.AreEqual(expected, a + b);
    }
}
