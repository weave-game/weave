using System.Reflection;
using Moq;
using Weave.Utils;

namespace WeaveTests;

public class UniqueNameGeneratorTests
{
    [Fact]
    public void New_GeneratesUniqueNamesIncludingBackup()
    {
        var generator = UniqueNameGenerator.Instance;

        // Get the Type of the instance
        var type = generator.GetType();
            
        // Retrieve PropertyInfo for the Suffixes property
        var propertyInfo = type.GetProperty("Suffixes", BindingFlags.NonPublic | BindingFlags.Instance);

        // Use GetValue to get the value of Suffixes
        var suffixes = (IEnumerable<string>)propertyInfo.GetValue(generator);

        generator.New();
        // This name should have backup index
        Assert.Equal("OnlyPrefixOnlySuffix0", generator.New());
    }
}