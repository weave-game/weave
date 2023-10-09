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

        // set the mockSuffixes to return the suffixes
        if (suffixes is List<string> l)
        {
            // remove all but one suffix
            while (l.Any())
                l.RemoveAt(0);
            
            l.Add("test");
        }
        else
        {
            Assert.Fail("Suffixes is not a List<string>, this test will not work");
        }

        generator.New();
        // This name should have backup index
        Assert.Equal("OnlyPrefixOnlySuffix0", generator.New());
    }
}