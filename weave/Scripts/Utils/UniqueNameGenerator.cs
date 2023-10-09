using System.Collections.Generic;

namespace Weave.Utils;

public class UniqueNameGenerator
{
    private static ISet<string> UsedNames { get; } = new HashSet<string>();
    private static int _backupIndex;
    private const int MaxAttempts = 100;

    /// <summary>
    ///    A shared instance of the <see cref="UniqueNameGenerator" />.
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

    public string New()
    {
        var attemps = 0;
        var name = Generate();

        while (UsedNames.Contains(name))
        {
            if (attemps >= MaxAttempts)
            {
                name = $"{Generate()} {_backupIndex++}";
                break;
            }

            name = Generate();
            attemps++;
        }

        UsedNames.Add(name);
        return name;
    }

    private string Generate()
    {
        var prefix = Prefixes.Random();
        var suffix = Suffixes.Random();
        return prefix + suffix;
    }
}
