namespace Leonardo.Tests;

public class UnitTestFibonacci
{
    [Fact]
    public async Task RunAsyncShouldRetrun8()
    {
        var result = await Fibonacci.RunAsync(new[] { "6" });
        Assert.Equal(8,  result[0]);
    }
}