using System;
using System.Reflection;
using UnityEngine;

[CreateAssetMenu(menuName = "Components/GPU", fileName = "GPU")]
[Serializable]
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
}