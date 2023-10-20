using System;
using System.Collections.Generic;
using System.Linq;

namespace Weave.Utils;

public sealed class UniqueNameGenerator
{
    private const int MaxAttempts = 100;
    private readonly IDictionary<string, int> _backupIndices = new Dictionary<string, int>();
    private ISet<string> UsedNames { get; } = new HashSet<string>();

    /// <summary>
    ///     A shared instance of the <see cref="UniqueNameGenerator" />.
    /// </summary>
    public static UniqueNameGenerator Instance { get; } = new();

    private IEnumerable<string> Prefixes { get; } =
        new List<string>
        {
            "Red",
            "Sinister",
            "Wicked",
            "Violet",
            "Cruel",
            "Dreaded",
            "Malevolent",
            "Shadowy",
            "Gloomy",
            "Bleak",
            "Ominous",
            "Twisted",
            "Corrupt",
            "Ruthless",
            "Venomous",
            "Fiery",
            "Murky",
            "Midnight",
            "Savage",
            "Noxious",
            "Rancid",
            "Poisoned"
        };

    private IEnumerable<string> Suffixes { get; } =
        new List<string>
        {
            "Lemurs",
            "Baboons",
            "Capuchins",
            "Marmosets",
            "Tarsiers",
            "Macaques",
            "Gibbons",
            "Mandrills",
            "Chimpanzees",
            "Orangutans",
            "Gorillas",
            "AyeAyes",
            "Iguanas",
            "Komodos",
            "Cheetahs",
            "Ocelots",
            "Jaguars",
            "Pangolins",
            "Kinkajous",
            "Bushbabys",
            "Tamarins",
            "Sifakas",
            "Langurs",
            "Guerezsa"
        };

    private IEnumerable<string> AnotherList { get; } = new List<string>
    {
        "A",
        "B",
        "C",
        "D",
        "E",
        "F",
        "G",
        "H",
        "I",
        "J"
    };

    public string New()
    {
        var attempts = 0;
        string name;

        do
        {
            name = Generate();
        } while (UsedNames.Contains(name) && ++attempts < MaxAttempts);

        if (UsedNames.Contains(name))
        {
            if (!_backupIndices.TryGetValue(name, out _))
                _backupIndices[name] = 0;

            _backupIndices[name]++;
            name += $" {_backupIndices[name]}";
        }

        UsedNames.Add(name);
        return name;
    }

    private string Generate()
    {
        var other = GetRandomElement(AnotherList);
        var prefix = GetRandomElement(Prefixes);
        var suffix = GetRandomElement(Suffixes);
        return other + prefix + suffix;
    }

    private readonly Random _random = new();

    private T GetRandomElement<T>(IEnumerable<T> enumerable)
    {
        if (enumerable == null)
        {
            throw new ArgumentNullException(nameof(enumerable));
        }

        var list = enumerable as IList<T> ?? enumerable.ToList();

        if (!list.Any())
        {
            throw new InvalidOperationException("The enumerable is empty.");
        }

        var index = _random.Next(list.Count);
        return list[index];
    }
}
