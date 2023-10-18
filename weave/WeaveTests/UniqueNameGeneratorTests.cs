using System.Reflection;
using Weave.Utils;

namespace WeaveTests;

public sealed class UniqueNameGeneratorTests
{
    [Fact]
    private void GeneratesRandomNames()
    {
        const int times = 10_000;
        var generatedNames = new List<string>();

        for (var i = 0; i < times; i++)
        {
            var name = UniqueNameGenerator.Instance.New();
            Assert.DoesNotContain(name, generatedNames);
            generatedNames.Add(name);
        }
    }

    [Fact]
    private void UsesBackupId()
    {
        var generator = UniqueNameGenerator.Instance;
        const string prefix = "prefix";
        const string suffix = "suffix";

        var type = generator.GetType();
        var backupIndexField = type.GetField("_backupIndex", BindingFlags.NonPublic | BindingFlags.Instance);
        var prefixesProperty = type.GetProperty("Prefixes", BindingFlags.NonPublic | BindingFlags.Instance);
        var suffixesProperty = type.GetProperty("Suffixes", BindingFlags.NonPublic | BindingFlags.Instance);
        var prefixes = (IEnumerable<string>)prefixesProperty?.GetValue(generator)!;
        var suffixes = (IEnumerable<string>)suffixesProperty?.GetValue(generator)!;

        // set backup index
        backupIndexField?.SetValue(generator, 1);

        if (prefixes is List<string> prefixesList)
        {
            while (prefixesList.Any())
                prefixesList.RemoveAt(0);

            prefixesList.Add(prefix);
        }
        else
        {
            Assert.Fail("Prefixes is not a List<string>, this test will not work");
        }

        if (suffixes is List<string> suffixesList)
        {
            while (suffixesList.Any())
                suffixesList.RemoveAt(0);

            suffixesList.Add(suffix);
        }
        else
        {
            Assert.Fail("Suffixes is not a List<string>, this test will not work");
        }

        Assert.Equal(prefix + suffix, generator.New());
        Assert.Equal(prefix + suffix + " 1", generator.New());
        Assert.Equal(prefix + suffix + " 2", generator.New());
    }
}