using System.Reflection;
using Godot;
using Weave.Utils;
using Xunit.Abstractions;

namespace WeaveTests;

public sealed class UniqueNameGeneratorTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public UniqueNameGeneratorTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    private void GeneratesRandomNames()
    {
        const int Times = 10_000;
        var generatedNames = new List<string>();

        for (var i = 0; i < Times; i++)
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
        const string Prefix = "prefix";
        const string Suffix = "suffix";

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
            {
                prefixesList.RemoveAt(0);
            }

            prefixesList.Add(Prefix);
        }
        else
        {
            Assert.Fail("Prefixes is not a List<string>, this test will not work");
        }

        if (suffixes is List<string> suffixesList)
        {
            while (suffixesList.Any())
            {
                suffixesList.RemoveAt(0);
            }

            suffixesList.Add(Suffix);
        }
        else
        {
            Assert.Fail("Suffixes is not a List<string>, this test will not work");
        }

        Assert.Equal(Prefix + Suffix, generator.New());
        Assert.Equal(Prefix + Suffix + " 1", generator.New());
        Assert.Equal(Prefix + Suffix + " 2", generator.New());
    }

    [Fact]
    private void ShouldBeSufficientlyRandom()
    {
        // NOTE: This is actually NOT a "fact" and is not guaranteed. Does not prove anything, just a "sanity" check that it's "sufficiently" random.
        const int N = 300_000;
        var instance = new UniqueNameGenerator();

        _testOutputHelper.WriteLine(
            $"Generating {N} names..."
        );

        var generatedNames = new List<string>();
        for (var i = 0; i < N; i++)
        {
            var n = instance.New();
            generatedNames.Add(n);
        }

        // Count amount that has numbers
        var nBackupIndex = generatedNames.Count(name => name.Any(char.IsDigit));
        var percentageBackupIndex = ((double)nBackupIndex / N * 100).ToString("0.00");

        _testOutputHelper.WriteLine(
            $"Found {nBackupIndex} names with backup index ({percentageBackupIndex}%)"
        );

        _testOutputHelper.WriteLine(
            $"Total unique: {N - nBackupIndex}"
        );

        Assert.Equal(N, generatedNames.Count);
    }
}
