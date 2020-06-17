using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Components/GPU", fileName = "GPU")]
public class GPU : PCComponent
{
    public short memory, TDP, power;
    public byte busVersion, busMultiplier;

    public override string FullProperties
    {
        get
        {
            string result = null;

            result += $"{fullName};\n";
            result += $"{LangManager.GetString("memory:")} {memory} MB;\n";
            result += $"TDP: {TDP} W;\n";

            result += $"{LangManager.GetString("interface:")} PCIe {busVersion}.0 x{busMultiplier};\n";

            result += $"{LangManager.GetString("performance:")} {power} GFlops;\n";
            result += $"{LangManager.GetString("price:")} {price}$.";

            return result;
        }
    }

    public override string ShortProperties => FullProperties;

    /// <summary>
    /// Clones this object.
    /// </summary>
    /// <returns>
    /// Clonned object.
    /// </returns>
    public override object Clone()
    {
        PCComponent component = ScriptableObject.CreateInstance<GPU>();
        component.fullName = this.fullName;
        component.shortName = this.shortName;
        component.price = this.price;
        component.time = this.time;
        component.rarity = this.rarity;
        component.image = this.image;

        ((GPU)component).memory = this.memory;
        ((GPU)component).TDP = this.TDP;
        ((GPU)component).power = this.power;
        ((GPU)component).busVersion = this.busVersion;
        ((GPU)component).busMultiplier = this.busMultiplier;

        return component;
    }
}