using System.Collections.Generic;
using System.Text;

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

    private IEnumerable<string> A { get; } =
        new List<string>
        {
            "Mega",
            "Uber",
            "Ultra",
            "Super",
            "Hyper",
            "Mighty",
            "Giga",
            "Tera",
            "Epic",
            "Grand"
        };

    private IEnumerable<string> B { get; } =
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

    private IEnumerable<string> C { get; } =
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
        var output = new StringBuilder();
        var prefixes = new List<string>();

        if (GsRandom.CoinToss())
            prefixes.Add(A.Random());

        if (GsRandom.CoinToss())
            prefixes.Add(B.Random());

        // Possible prefixes
        output.Append(string.Concat(prefixes.Shuffled()));

        return output.Append(C.Random()).ToString();
    }
}
