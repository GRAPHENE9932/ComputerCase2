using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Chipsets supported overclock: X370, B350, X300, A300 (AMD) and ... (Intel)
public enum Chipset
{
    Z390, Z370, Z270, Z170, X570, X99, X470, H110, H270, B150, B250, B360, B365, H370, _990X
}

[CreateAssetMenu(menuName = "Components/Motherboard", fileName = "Motherboard")]
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

    /// <summary>
    /// Clones this object.
    /// </summary>
    /// <returns>
    /// Clonned object.
    /// </returns>
    public override object Clone()
    {
        PCComponent component = ScriptableObject.CreateInstance<Motherboard>();
        component.fullName = this.fullName;
        component.shortName = this.shortName;
        component.price = this.price;
        component.time = this.time;
        component.rarity = this.rarity;
        component.image = this.image;

        ((Motherboard)component).socket = this.socket;
        ((Motherboard)component).RAMType = this.RAMType;
        ((Motherboard)component).RAMCount = this.RAMCount;
        ((Motherboard)component).busVersions = this.busVersions;
        ((Motherboard)component).busMultipliers = this.busMultipliers;
        ((Motherboard)component).SLI = this.SLI;
        ((Motherboard)component).crossfire = this.crossfire;
        ((Motherboard)component).chipset = this.chipset;

        return component;
    }
}
