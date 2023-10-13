using Weave.Utils;

namespace WeaveTests;

public sealed class UniqueColorGeneratorTests
{
    [Fact]
    private void IsDeterministic()
    {
        var instance1 = new UniqueColorGenerator();
        var instance2 = new UniqueColorGenerator();

        for (var i = 0; i < 1000; i++)
        {
            var color1 = instance1.NewColor();
            var color2 = instance2.NewColor();
            Assert.Equal(color1, color2);
        }
    }
}