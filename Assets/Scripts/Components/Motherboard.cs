using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using KlimSoft;

public enum Chipset
{
    Z390, Z370, Z270, Z170, X570, X99, X470, H110, H270, B150, B250, B360, B365, H370, _990X, Z490, X299,
    X399, P35, VIA_P4M800
}

[CreateAssetMenu(menuName = "Components/Motherboard", fileName = "Motherboard")]
[Serializable]
public class Motherboard : PCComponent
{
    public string socket;
    public byte RAMType, RAMCount;
    public byte[] busVersions, busMultipliers;
    public bool SLI, crossfire;
    public Chipset chipset;

    public override string FullProperties
    {
        get
        {
            string result = null;

            result += $"{fullName};\n";
            result += $"{LangManager.GetString("socket:")} {socket};\n";

            if (RAMType == 1)
                result += $"{LangManager.GetString("ram_gen:")} DDR;\n";
            else
                result += $"{LangManager.GetString("ram_gen:")} DDR{RAMType};\n";

            result += $"{LangManager.GetString("ram_count:")} {RAMCount};\n";

            result += $"{LangManager.GetString("gpu_interfaces:")}\n";
            string interfaces = null;
            int countOfType = 1;
            for (int i = busVersions.Length - 2; i >= -1; i--)
            {
                if (i >= 0 && busVersions[i] == busVersions[i + 1] && busMultipliers[i] == busMultipliers[i + 1])
                {
                    countOfType++;
                }
                else
                {
                    interfaces = $"{countOfType}x PCIe {busVersions[i + 1]}.0 x{busMultipliers[i + 1]};\n" + interfaces;
                    countOfType = 1;
                }
            }
            result += interfaces;

            result += "SLI: " + LangManager.GetString(SLI ? "yes" : "no") + ";\n";
            result += "Crossfire: " + LangManager.GetString(crossfire ? "yes" : "no") + ";\n";
            result += $"{LangManager.GetString("chipset:")} {chipset.ToString().RemoveChar('_')};\n";
            result += $"{LangManager.GetString("price:")} {price}$.";

            return result;
        }
    }

    public override string ShortProperties
    {
        get
        {
            string result = null;

            result += $"{fullName};\n";
            result += $"{LangManager.GetString("socket:")} {socket};\n";

            if (RAMType == 1)
                result += $"{LangManager.GetString("ram_gen:")} DDR;\n";
            else
                result += $"{LangManager.GetString("ram_gen:")} DDR{RAMType};\n";

            result += $"{LangManager.GetString("ram_count:")} {RAMCount};\n";

            result += "SLI: " + LangManager.GetString(SLI ? "yes" : "no") + ";\n";
            result += "Crossfire: " + LangManager.GetString(crossfire ? "yes" : "no") + ";\n";
            result += $"{LangManager.GetString("chipset:")} {chipset.ToString().RemoveChar('_')};\n";
            result += $"{LangManager.GetString("price:")} {price}$.";

            return result;
        }
    }

    public bool SupportsCPUOverclocking
    {
        get
        {
            Chipset[] supChipsets = new Chipset[] {Chipset.Z490, Chipset.Z390, Chipset.Z370,
            Chipset.Z270, Chipset.Z170, Chipset.X299, Chipset.X399, Chipset.X470,
            Chipset.X570, Chipset.X99, Chipset.B365, Chipset.B360};

            return supChipsets.Contains(chipset);
        }
    }
}
