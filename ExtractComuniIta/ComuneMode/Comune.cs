using System;
using System.Collections.Generic;

namespace ExtractComuniIta.ComuneMode;

public class Comune
{
    public string NomComune { get; set; }
    public string NomSansAccent { get; set; }
    public string Province { get; set; }
    public string Region { get; set; }

    public Comune(string nomComune, string province, string region)
    {
        NomComune = nomComune;
        NomSansAccent = nomComune.RemoveDiacritics();
        Province = province;
        Region = region;
    }

    private sealed class NomSansAccentRelationalComparer : IComparer<Comune>
    {
        public int Compare(Comune x, Comune y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(null, x)) return -1;
            return string.Compare(x.NomSansAccent, y.NomSansAccent, StringComparison.Ordinal);
        }
    }

    public static IComparer<Comune> NomSansAccentComparer { get; } = new NomSansAccentRelationalComparer();

    public override string ToString()
    {
        return $"{NomComune} - {Province}";
    }
}