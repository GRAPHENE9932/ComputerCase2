using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GPUInterface
{
    PCIe_4_0_x16, PCIe_4_0_x8, PCIe_3_0_x16, PCIe_3_0_x8, PCIe_3_0_x4, PCIe_3_0_x1, PCIe_2_0_x1, PCIe_4_0_x4, PCIe_4_0_x1, PCIe_2_0_x4, PCIe_1_0_x16,
    PCIe_1_0_x1, PCIe_3_0_x2, PCIe_2_0_x16, PCIe_2_0_x8
}
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
            result += $"Memory: {memory} MB;\n";
            result += $"TDP: {TDP} W;\n";

            result += $"Interface: PCIe {busVersion}.0 x{busMultiplier};\n";

            result += $"Performance: {power} GFlops;\n";
            result += $"Price: {price}$.";

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
    public override PCComponent Clone()
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