using System;
using System.Collections.Generic;
using System.Linq;

namespace ExtractComuniIta.ComuneMode;

public class DatiComuneItalia
{
    public Dictionary<string, Region> DicoRegion { get; set; } = new Dictionary<string, Region>();
    //public Dictionary<string, List<Comune>> DicoProvince { get; set; } = new Dictionary<string, List<Comune>>();

    public void Add(Comune comune)
    {
        if (!DicoRegion.ContainsKey(comune.Region))
        {
            DicoRegion.Add(comune.Region, new Region(comune.Region));
        }
        DicoRegion[comune.Region].Add(comune);


        //if (!DicoProvince.ContainsKey(comune.Province))
        //{
        //    DicoProvince.Add(comune.Province, new List<Comune>());
        //}
        //DicoProvince[comune.Province].Add(comune);
    }

    public List<Province> GetAllProvinces()
    {
        var regionList = DicoRegion.Values.OrderBy(v => v.Nom, StringComparer.OrdinalIgnoreCase).ToList();
        var allProv = new List<Province>();
        foreach (var region in regionList)
        {
           var prov=region.DicoProvince.Values.ToList();//.OrderBy(v => v.NomProvince, StringComparer.OrdinalIgnoreCase).ToList();
           foreach (var province in prov)
           {
               province.ListeComune.Sort(Comune.NomSansAccentComparer);
           }
           allProv.AddRange(prov);

        }

        return allProv;
    }
}

public class Region
{

    public string Nom { get; set; }
    public string NomSansAccent { get; set; }
    public Dictionary<string, Province> DicoProvince { get; set; } = new Dictionary<string, Province>();

    public Region(string nom)
    {
        NomSansAccent = nom.RemoveDiacritics();
        Nom = nom;
    }

    public void Add(Comune comune)
    {
        if (!DicoProvince.ContainsKey(comune.Province))
        {
            DicoProvince.Add(comune.Province, new Province(comune.Province));
        }

        DicoProvince[comune.Province].Add(comune);
    }

    public override string ToString()
    {
        return $"{Nom}";
    }
}

public class Province
{
    public string NomProvince { get; set; }
    public string NomSansAccent { get; set; }
    public List<Comune> ListeComune { get; set; } = new List<Comune>();

    public Province(string prov)
    {
        NomProvince = prov;
        NomSansAccent = prov.RemoveDiacritics();
    }

    public void Add(Comune comune)
    {
       ListeComune.Add(comune);
    }

    public override string ToString()
    {
        return $"{NomProvince}";
    }
}