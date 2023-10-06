using System;
using System.Collections.Generic;
using System.Linq;

namespace weave.Utils;

public static class UniqueNameGenerator
{
    private static ISet<string> UsedNames { get; } = new HashSet<string>();

    private static IReadOnlySet<string> Prefixes { get; } = new HashSet<string>
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

    private static IReadOnlySet<string> Suffixes { get; } = new HashSet<string>
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

    public static string New()
    {
        var name = Generate();

        while (UsedNames.Contains(name))
            name = Generate();

        UsedNames.Add(name);
        return name;
    }

    private static string Generate()
    {
        var prefix = Prefixes.Random();
        var suffix = Suffixes.Random();
        return $"{prefix}{suffix}";
    }

    private static T Random<T>(this IEnumerable<T> enumerable)
    {
        var rnd = new Random();
        var index = rnd.Next(0, enumerable.Count());
        return enumerable.ElementAt(index);
    }
}